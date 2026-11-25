using Microsoft.AspNetCore.Identity;

namespace MotoRent.Web.Data;

public static class IdentityRoleSeeder
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var roleName in new[] { "User", "Administrator" })
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                var errors = string.Join(", ",
                    result.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create the '{roleName}' role: {errors}");
            }
        }
    }
}