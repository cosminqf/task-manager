using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Areas.Identity.Data;
using TaskManager.Data;
using TaskManager.Models;

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
                    PasswordHash = hasher.HashPassword(new ApplicationUser(), "Member1!")
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

            // Seed Tasks - Atribuite utilizatorilor
            context.ProjectTasks.AddRange(
                new ProjectTask
                {
                    Title = "Initial Task 1",
                    Description = "This is the first seeded task.",
                    Status = TaskManager.Models.TaskStatus.NotStarted,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5),
                    AssignedUserId = "5b54ca8a-aba8-49bd-9a6e-503ae66fd5d1" // member@test.com
                },
                new ProjectTask
                {
                    Title = "Initial Task 2",
                    Description = "This is the second seeded task.",
                    Status = TaskManager.Models.TaskStatus.InProgress,
                    StartDate = DateTime.Now.AddDays(-2),
                    EndDate = DateTime.Now.AddDays(3),
                    MediaUrl = "https://www.youtube.com/embed/PErrvYtVzbk",
                    AssignedUserId = "5b54ca8a-aba8-49bd-9a6e-503ae66fd5d1" // member@test.com
                },
                new ProjectTask
                {
                    Title = "Cat in a fish bowl",
                    Description = "A funny video of a cat stuck in a fish bowl.",
                    Status = TaskManager.Models.TaskStatus.Completed,
                    StartDate = DateTime.Now.AddDays(-10),
                    EndDate = DateTime.Now.AddDays(-5),
                    MediaUrl = "https://www.youtube.com/embed/9FjGP4t2zKY",
                    AssignedUserId = "80cd74b1-da1e-48e7-a79a-cdd3cad6e7a8" // admin@test.com
                }
            );
                            
            context.SaveChanges();
                        
        }
        
    }

}

