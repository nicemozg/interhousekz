using English_games.Models;
using Microsoft.AspNetCore.Identity;

namespace English_games.Services;

public class AdminInitializer
{
    public static async Task SeedAdminUser(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        string adminEmail = "nice.mozg@gmail.com";
        string adminUserName = "7064441111";
        string password = "Legioner012788796!";
        string adminPhoneNumber = "77064441111";
        bool confirmedPhoneNumber = true;
        bool confirmedEmail = true;
        int balance = 100000;

        var roles = new[] { "superAdmin", "admin", "user", "block" };
        foreach (var role in roles)
        {
            if (await roleManager.FindByNameAsync(role) is null)
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (await userManager.FindByNameAsync(adminUserName) is null)
        {
            User superAdmin = new User { Email = adminEmail, UserName = adminUserName,
                AvatarFileName = "3be4e88b-0ce6-40d0-b7cf-e7cbaac8d3f6_photo_2023-01-25_01-21-38.jpg",
                Balance = balance, PhoneNumber = adminPhoneNumber, PhoneNumberConfirmed = confirmedPhoneNumber,
                EmailConfirmed = confirmedEmail
            };
            IdentityResult result = await userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(superAdmin, "superAdmin");
        }
    }
}