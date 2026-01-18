using Microsoft.AspNetCore.Identity;
using SweetOrder.Models;
using Microsoft.Extensions.DependencyInjection;

namespace SweetOrder.Data
{
    public static class Dbseeder
    {
        public static class DbSeeder
        {
            // 傳入 ServiceProvider 來取得需要的服務
            public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
            {
                // 取得使用者管理員 & 角色管理員
                var userManager = service.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();

                // 1. 確保 "Admin" 角色存在
                if (!await roleManager!.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                // 確保 "Member" 角色存在
                if (!await roleManager.RoleExistsAsync("Member"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Member"));
                }

                // 2. 建立管理員帳號 (如果不存在)
                var adminEmail = "admin@sweetorder.com"; // ★ 這是你的後台帳號
                var adminUser = await userManager!.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var newAdmin = new IdentityUser
                    {

                        Email = adminEmail,
                        UserName = "超級管理員",
                        EmailConfirmed = true

                    };
                    
                    await userManager.CreateAsync(newAdmin, "admin1102");

                    // ★ 把他加入 Admin 角色
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
                else
                {
                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }
            }
        }
    }
}
