namespace Data;

using System;
using System.Threading.Tasks;
using Data.Entities;
using Data.Enums;
using Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using SharjahConferenceAPI.Services.Interfaces;

public static class DbInitializer
{
    public static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        const string adminEmail = "admin@sharjahconference.ae";
        var existingAdmin = await unitOfWork.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == adminEmail);

        if (existingAdmin == null)
        {
            var adminUser = new User
            {
                Email = adminEmail,
                PasswordHash = passwordHasher.HashPassword("Admin@123456"),
                Role = UserRole.Admin
            };

            await unitOfWork.Users.AddAsync(adminUser);
            await unitOfWork.CompleteAsync();
        }
    }
}
