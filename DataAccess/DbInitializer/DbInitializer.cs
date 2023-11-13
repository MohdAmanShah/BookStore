using DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        public readonly AppDbContext appDbContext;
        public readonly RoleManager<IdentityRole> roleManager;
        public readonly UserManager<ApplicationUser> userManager;
        public DbInitializer(AppDbContext appDbContext, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this.appDbContext = appDbContext;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }
        public void Initialize()
        {
            try
            {
                if (appDbContext.Database.GetPendingMigrations().Count() > 0)
                {
                    appDbContext.Database.Migrate();
                }
            }
            catch { }
            if (!roleManager.RoleExistsAsync(StaticData.Admin_Role).GetAwaiter().GetResult())
            {
                roleManager.CreateAsync(new IdentityRole(StaticData.Admin_Role)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(StaticData.Cust_Role)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(StaticData.Comp_Role)).GetAwaiter().GetResult();
                roleManager.CreateAsync(new IdentityRole(StaticData.Admin_Role)).GetAwaiter().GetResult();

                userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "reach.MohdAmanShah@gmail.com",
                    Email = "reach.MohdAmanShah@gmail.com",
                    Name = "Mohd Aman Shah",
                    PhoneNumber = "7310976105",
                    Street = "Near Dakpathar Bus Stand",
                    State = "uttarakhand",
                    Postal = "248115,",
                    City = "VikasNagar"
                }, "Admin@123").GetAwaiter().GetResult();
                ApplicationUser user = appDbContext.ApplicationUsers.FirstOrDefault(i => i.Email == "reach.MohdAmanShah@gmail.com");
                userManager.AddToRoleAsync(user, StaticData.Admin_Role).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
