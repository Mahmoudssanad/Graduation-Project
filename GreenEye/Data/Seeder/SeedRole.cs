using GreenEye.Enums;
using Microsoft.AspNetCore.Identity;

namespace GreenEye.Data.Seeder
{
    public class SeedRole
    {
        public async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = Enum.GetNames(typeof(Roles));

            foreach(var role in roles)
            {
                if(!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
