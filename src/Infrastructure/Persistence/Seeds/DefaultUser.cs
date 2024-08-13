using Application.Common.Enums;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Seeds
{
    public static class DefaultUser
    {
        public static async Task SeedAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            if (!context.Users.Any())
            {
                


                    //LLenamos el User Admin por defecto
                    var defaultUser = new User
                    {
                        UserName = "userAdmin",
                        Email = "userAdmin@gmail.com",
                        FirstName = "Pepe",
                        LastName = "Pepardo",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    if (userManager.Users.All(u => u.Id != defaultUser.Id))
                    {
                        var user = await userManager.FindByEmailAsync(defaultUser.Email);
                        if (user == null)
                        {
                            await userManager.CreateAsync(defaultUser, "P4ssw0rd!");
                            await userManager.AddToRoleAsync(defaultUser, Roles.Admin.ToString());
                            await userManager.AddToRoleAsync(defaultUser, Roles.Basic.ToString());
                        }
                    }

                    //LLenamos el User Basic por defecto
                    var defaultBasicUser = new User
                    {
                        UserName = "userBasic",
                        Email = "userBasic@gmail.com",
                        FirstName = "Pepito",
                        LastName = "Pepitten",
                        EmailConfirmed = true,
                        PhoneNumberConfirmed = true
                    };

                    if (userManager.Users.All(u => u.Id != defaultBasicUser.Id))
                    {
                        var user = await userManager.FindByEmailAsync(defaultBasicUser.Email);
                        if (user == null)
                        {
                            await userManager.CreateAsync(defaultBasicUser, "P4ssw0rd!");
                            await userManager.AddToRoleAsync(defaultBasicUser, Roles.Basic.ToString());
                        }
                    }
                
            }

        }
    }
}