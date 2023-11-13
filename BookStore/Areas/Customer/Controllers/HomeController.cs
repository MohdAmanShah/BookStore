using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using Utility;
namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                HttpContext.Session.SetInt32(StaticData.SessionCart, unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == claim.Value).Count());
            }
            IEnumerable<ProductModel> products = unitOfWork.ProductRepository.GetAll(includeProp: "Category");
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Details(int id)
        {
            ShoppingCartModel model = new ShoppingCartModel
            {
                Product = unitOfWork.ProductRepository.Get(u => u.ProductId == id, includeProp: "Category"),
                Count = 1,
                ProductId = id
            };
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCartModel model)
        {
            ClaimsIdentity? claimIdentity = (ClaimsIdentity)User.Identity;
            if (claimIdentity != null)
            {
                var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                model.UserId = UserId;
            }
            if (ModelState.IsValid)
            {
                var DbObj = unitOfWork.ShopingCartRepository.Get(u => u.UserId == model.UserId && u.ProductId == model.ProductId);
                if (DbObj != null)
                {
                    DbObj.Count += model.Count;
                    unitOfWork.ShopingCartRepository.Update(DbObj);
                }
                else
                {
                    unitOfWork.ShopingCartRepository.Add(model);
                }
                unitOfWork.Commit();
                HttpContext.Session.SetInt32(StaticData.SessionCart, unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == model.UserId).Count());
                TempData["success"] = "Cart updated successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Cart was not updated - Invalid Details";
            return RedirectToAction("Details", model.ProductId);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}