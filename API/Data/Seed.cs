using System;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using API.Entities;
using API.DTOs;
using Microsoft.AspNetCore.Identity;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;
        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members is null)
        {
            Console.WriteLine("No members to seed.");
            return;
        }


        foreach (var member in members)
        {
            var user = new AppUser
            {
                Id = member.Id,
                DisplayName = member.DisplayName,
                Email = member.Email,
                ImageUrl = member.ImageUrl,
                UserName = member.Email,
                Member = new Member
                {
                    Id = member.Id,
                    DisplayName = member.DisplayName,
                    Description = member.Description,
                    DateOfBirth = member.DateOfBirth,
                    ImageUrl = member.ImageUrl,
                    Gender = member.Gender,
                    City = member.City,
                    Country = member.Country,
                    Created = member.Created,
                    LastActive = member.LastActive,
                }
            };

            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                MemberId = member.Id
            });

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");
            if(!result.Succeeded)
            {
                System.Console.WriteLine(result.Errors.First().Description);
            }
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser
        {
           UserName = "admin@test.com",
           Email= "admin@test.com",
           DisplayName = "Admin"
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin,["Admin", "Moderator"]);

    }
}
