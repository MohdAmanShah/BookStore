using DataAccess;
using DataAccess.Repository.Interfaces;
using DataModels;
using DataModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticData.Admin_Role)]
    public class UserController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        public UserController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Permission(String Id)
        {
            var UserToRoles = db.UserRoles.ToList();
            var Roles = db.Roles.ToList();
            var RoleId = UserToRoles.FirstOrDefault(i => i.UserId == Id).RoleId;

            UserPermissionViewModel model = new UserPermissionViewModel
            {
                User = db.ApplicationUsers.Where(i => i.Id == Id).Include(i => i.Company).FirstOrDefault(),
                Roles = Roles.Select(i => i.Name).ToList(),
                Companies = db.Companies.ToList(),
            };
            model.User.Role = Roles.Where(i => i.Id == RoleId).FirstOrDefault().Name;
            return View(model);
        }
        [HttpPost]
        public IActionResult Permission(UserPermissionViewModel model)
        {
            var User = db.ApplicationUsers.Where(i => i.Id == model.User.Id).FirstOrDefault();
            if (User == null)
            {
                TempData["success"] = "User not found";
                return RedirectToAction(nameof(Index));
            }

            User.CompanyId = (model.User.Role == StaticData.Comp_Role)? model.User.CompanyId : null;
            var prevRole = userManager.GetRolesAsync(User).GetAwaiter().GetResult();
            userManager.RemoveFromRoleAsync(User, prevRole.FirstOrDefault()).GetAwaiter().GetResult();
            userManager.AddToRoleAsync(User, model.User.Role).GetAwaiter().GetResult();
            db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> users = db.ApplicationUsers.Include(i => i.Company).ToList();
            var UsersToRoles = db.UserRoles.ToList();
            var Roles = db.Roles.ToList();
            foreach (var user in users)
            {
                if (user.Company == null)
                {
                    user.Company = new CompanyModel { Name = String.Empty };
                }
                var RoleId = UsersToRoles.FirstOrDefault(i => i.UserId == user.Id).RoleId;
                var Role = Roles.FirstOrDefault(i => i.Id == RoleId);
                user.Role = Role.Name;
            }
            return Json(new
            {
                data = users,
            });
        }
        [HttpPost]
        public IActionResult Lockout([FromBody] string id)
        {
            String Message = String.Empty;
            var user = db.ApplicationUsers.FirstOrDefault(i => i.Id == id);
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Error while locking/unlocking"
                });
            }
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = DateTime.Now;
                Message = "User unlocked successfully";

            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(1000);
                Message = "User locked successfully";
            }
            db.SaveChanges();
            return Json(new
            {
                success = true,
                message = Message
            });

        }
        #endregion
    }
}
