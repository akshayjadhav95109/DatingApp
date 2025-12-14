using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MembersController(IUnitOfWork unitOfWork, IPhotoService photoService) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Member>>> GetMembers([FromQuery] MemberParams memberParams)
        {
            memberParams.CurrentMemberId = User.GetMemberId();
            var members = await unitOfWork.MemberRepository.GetMembersAsync(memberParams);
            return Ok(members);
        }

        [HttpGet("{id}")] //localhost:5001/api/members/bob-id
        public async Task<ActionResult<Member>> GetMember(string id)
        {
            var member = await unitOfWork.MemberRepository.GetMemberByIdAsync(id);
            return member == null ? NotFound() : member;
        }

        [HttpGet("{id}/photos")]

        public async Task<ActionResult<IReadOnlyList<Photo>>> GetMemberPhotos(string id)
        {
            var photos = await unitOfWork.MemberRepository.GetPhotoForMemberAsync(id);
            return Ok(photos);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var memberId = User.GetMemberId();

            var member = await unitOfWork.MemberRepository.GetMemberForUpdate(memberId);

            if (member == null) return BadRequest("Member not found");

            member.DisplayName = memberUpdateDto.DisplayName ?? member.DisplayName;
            member.Description = memberUpdateDto.Description ?? member.Description;
            member.City = memberUpdateDto.City ?? member.City;
            member.Country = memberUpdateDto.Country ?? member.Country;

            member.User.DisplayName = memberUpdateDto.DisplayName ?? member.User.DisplayName; //Sync User's DisplayName

            unitOfWork.MemberRepository.Update(member); //Optional

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update member");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto([FromForm] IFormFile file)
        {
            var member = await unitOfWork.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Member not found");

            var result = await photoService.UploadPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId,
                MemberId = member.Id
            };

            if (member.ImageUrl == null)
            {
                member.ImageUrl = photo.Url;
                member.User.ImageUrl = photo.Url;
            }

            member.Photos.Add(photo);

            if (await unitOfWork.Complete()) return photo;

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var member = await unitOfWork.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Member not found");

            var photo = member.Photos.SingleOrDefault(x=> x.Id == photoId);

            if(member.ImageUrl == photo?.Url || photo == null) 
                return BadRequest("Photo not found or is already the main photo");
            
            member.ImageUrl = photo.Url;
            member.User.ImageUrl = photo.Url;

            if (await unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var member = await unitOfWork.MemberRepository.GetMemberForUpdate(User.GetMemberId());

            if (member == null) return BadRequest("Member not found");

            var photo = member.Photos.SingleOrDefault(x=> x.Id == photoId);

            if(photo == null || photo.Url == member.ImageUrl) 
                return BadRequest("This photo cannot be deleted");

            if(photo.PublicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            member.Photos.Remove(photo);
            if (await unitOfWork.Complete()) return Ok();


            return BadRequest("Failed to delete photo");
        }
    }
}
