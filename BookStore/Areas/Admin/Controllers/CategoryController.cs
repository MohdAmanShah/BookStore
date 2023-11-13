using DataAccess.Repository.Interfaces;
using DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticData.Admin_Role)]
    public class CategoryController : Controller
    {
        private IUnitOfWork unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Category());
            }
            Category _Category = unitOfWork.CategoryRepository.Get(u => u.Id == id);
            if (_Category == null)
            {
                TempData["error"] = "Could not find data";
                return RedirectToAction("Index");
            }
            return View(_Category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Failed to insert data : Invalid Data";
                return View(new Category());
            }
            else
            {
                if (category.Id == 0)
                {
                    unitOfWork.CategoryRepository.Add(category);
                    TempData["success"] = "Data added successfully";
                }
                else
                {
                    unitOfWork.CategoryRepository.Update(category);
                    TempData["success"] = "Data updated successfully";
                }
                unitOfWork.Commit();
                return RedirectToAction("Index");
            }
        }

        #region API CALLS
        [HttpGet]
        public IActionResult getAll()
        {
            List<Category> _categories = unitOfWork.CategoryRepository.GetAll().ToList();
            return Json(new { data = _categories });
            #endregion
        }
        [HttpDelete]
        public IActionResult delete(int id)
        {
            if (id == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "could not delete - invalid Id"
                });
            }
            Category _Category = unitOfWork.CategoryRepository.Get(u => u.Id == id);
            if (_Category == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Could not delete - Object not found"
                });
            }
            unitOfWork.CategoryRepository.Remove(_Category);
            unitOfWork.Commit();
            return Json(new
            {
                success = true,
                message = "Data deleted successfully"
            });
        }
    }
}
