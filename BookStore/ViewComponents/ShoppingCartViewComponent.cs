using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Utility;

namespace BookStore.ViewComponents
{
    public class ShoppingCartViewComponent:ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            Claim claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim != null)
            {
                if(HttpContext.Session.GetInt32(StaticData.SessionCart) == null)
                {
                    HttpContext.Session.SetInt32(StaticData.SessionCart, _unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(StaticData.SessionCart));
            }
            HttpContext.Session.Clear();
            return View(0);

        }
    }
}
