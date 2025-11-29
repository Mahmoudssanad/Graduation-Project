namespace GreenEye.Data.Seeder
{
    public static class SeedAdmin
    {
        public async static Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
        {
            if(await userManager.FindByEmailAsync("Admin@gmail.com") == null)
            {
                var admin = new ApplicationUser
                {
                    Email = "Admin@gmail.com",
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            
        }
    }
}
