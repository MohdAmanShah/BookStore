using DataAccess.Repository.Interfaces;
using DataModels;
using DataModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using Utility;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticData.Admin_Role)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new CompanyModel { Name = "" });
            }
            CompanyModel model = unitOfWork.CompanyRepository.Get(u => u.Id == id);
            if (model == null)
            {
                TempData["error"] = "Could not find the data";
                return RedirectToAction("Index");
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CompanyModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    unitOfWork.CompanyRepository.Add(model);
                    TempData["success"] = "Data Added successfully";
                }
                else
                {
                    unitOfWork.CompanyRepository.Update(model);
                    TempData["success"] = "Data Updated successfully";
                }
                unitOfWork.Commit();
                return RedirectToAction("Index");

            }
            TempData["error"] = "Failed: Invalid Data";
            return View(model);
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<CompanyModel> products = unitOfWork.CompanyRepository.GetAll();
            return Json(new
            {
                data = products
            });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            CompanyModel Company = unitOfWork.CompanyRepository.Get(u => u.Id == id);
            if (Company != null)
            {
                unitOfWork.CompanyRepository.Remove(Company);
                unitOfWork.Commit();
                return Json(new
                {
                    success = true,
                    message = "Data deleted successfully"
                });
            }
            return Json(new
            {
                success = false,
                message = "Could'nt delete - file not found"
            });

        }
        #endregion
    }
}
