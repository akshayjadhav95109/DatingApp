using System;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MemberRepository (AppDbContext _context) : IMemberRepository
{
    public async Task<Member?> GetMemberByIdAsync(string id)
    {
        return await _context.Members.FindAsync(id);
    }

    public async Task<Member?> GetMemberForUpdate(string id)
    {
        return await _context.Members.Include(x=>x.User).Include(x=>x.Photos).SingleOrDefaultAsync(x=>x.Id==id);
    }

    public async Task<PaginatedResults<Member>> GetMembersAsync(MemberParams memberParams)
    {
        var query = _context.Members.AsQueryable();
        query = query.Where(x=>x.Id != memberParams.CurrentMemberId);

        if(memberParams.Gender != null)
        {
            query = query.Where(x=> x.Gender == memberParams.Gender);
        }
    
        var minDob = DateOnly.FromDateTime(DateTime.Today).AddYears(-memberParams.MaxAge -1);
        var maxDob = DateOnly.FromDateTime(DateTime.Today).AddYears(-memberParams.MinAge);

        query = query.Where(x=>x.DateOfBirth>=minDob && x.DateOfBirth<=maxDob);

        query = memberParams.OrderBy switch
        {
            "created" => query.OrderByDescending(x=> x.Created),
            _ => query.OrderByDescending(x=> x.LastActive)
        };

        return await PaginationHelper.CreateAsync(query, memberParams.PageNumber, memberParams.PageSize);
        
    }

    public async Task<IReadOnlyList<Photo>> GetPhotoForMemberAsync(string memberId)
    {
        return await _context.Members.Where(x => x.Id == memberId)
                .SelectMany(x => x.Photos).ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Update(Member member)
    {
        _context.Entry(member).State = EntityState.Modified;
    }
}
