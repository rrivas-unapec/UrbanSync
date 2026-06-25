using Microsoft.AspNetCore.Identity;
using UrbanSync.Web.Models;

namespace UrbanSync.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles =
        {
            "Administrador",
            "Supervisor",
            "Tecnico",
            "Ciudadano"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await CreateUserAsync(
            userManager,
            "admin@urbansync.com",
            "Admin123*",
            "Administrador UrbanSync",
            "00000000000",
            "Administrador General",
            "Administrador"
        );

        await CreateUserAsync(
            userManager,
            "supervisor@urbansync.com",
            "Supervisor123*",
            "Supervisor Municipal",
            "00100000001",
            "Supervisor de Operaciones",
            "Supervisor"
        );

        await CreateUserAsync(
            userManager,
            "tecnico@urbansync.com",
            "Tecnico123*",
            "Técnico de Infraestructura",
            "00100000002",
            "Técnico de Reparaciones",
            "Tecnico"
        );

        await CreateUserAsync(
            userManager,
            "ciudadano@urbansync.com",
            "Ciudadano123*",
            "Ciudadano de Prueba",
            "00100000003",
            "Ciudadano",
            "Ciudadano"
        );
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string identificationNumber,
        string position,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user != null)
            return;

        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            IdentificationNumber = identificationNumber,
            Position = position,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}