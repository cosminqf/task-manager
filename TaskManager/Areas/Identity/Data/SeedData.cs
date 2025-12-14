using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Areas.Identity.Data;
using TaskManager.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
        {
            if (context.Roles.Any())
                return;
            context.Roles.AddRange(
                new IdentityRole
                {
                    Id = "e93d014f-bd80-458b-bcb9-c29fdaf7cba6", Name = "Admin", NormalizedName = "Admin".ToUpper() 
                },

                new IdentityRole
                {
                    Id = "4aa12e41-b16f-432a-8166-62fe0543d1ef", Name = "Member", NormalizedName = "Member".ToUpper() }

                );

                var hasher = new PasswordHasher<ApplicationUser>();

            context.Users.AddRange(
                new ApplicationUser

                {

                    Id = "80cd74b1-da1e-48e7-a79a-cdd3cad6e7a8",
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,"Admin1!")
                },
                new ApplicationUser

                {

                    Id = "5b54ca8a-aba8-49bd-9a6e-503ae66fd5d1",
                    // primary key
                    UserName = "member@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "MEMBER@TEST.COM",
                    Email = "member@test.com",
                    NormalizedUserName = "MEMBER@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,"Member1!")
                }
            );
            context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {

                    RoleId = "e93d014f-bd80-458b-bcb9-c29fdaf7cba6",
                    UserId = "80cd74b1-da1e-48e7-a79a-cdd3cad6e7a8"
                },

                new IdentityUserRole<string>

                {

                    RoleId = "4aa12e41-b16f-432a-8166-62fe0543d1ef",
                    UserId = "5b54ca8a-aba8-49bd-9a6e-503ae66fd5d1"
                }
            );
                            
            context.SaveChanges();
                        
        }
        
    }

}

