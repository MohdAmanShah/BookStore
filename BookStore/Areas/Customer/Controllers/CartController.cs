using DataAccess.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using DataAccess.Repository.Interfaces;
using DataModels.ViewModels;
using System.Security.Claims;
using DataModels;
using System.Drawing.Drawing2D;
using Utility;
using Stripe.Checkout;
using Stripe;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        [BindProperty]
        public CartViewModel Cart { get; set; }
        private readonly IUnitOfWork unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            String UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            Cart = new CartViewModel
            {
                Items = unitOfWork.ShopingCartRepository.GetAll(u => u.UserId == UserId, includeProp: "Product"),
                orderHeader = new OrderHeader()
            };
            foreach (var Item in Cart.Items)
            {
                Item.Price = GetPrice(Item);
                Cart.orderHeader.OrderTotal += Item.Price * Item.Count;
            }
            HttpContext.Session.SetInt32(StaticData.SessionCart, unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == UserId).Count());
            return View(Cart);
        }

        private double GetPrice(ShoppingCartModel Item)
        {
            if (Item.Count <= 50)
            {
                return Item.Product.Price;
            }
            else if (Item.Count >= 50 && Item.Count < 100)
            {
                return Item.Product.Price50;
            }
            return Item.Product.Price100;
        }
        public IActionResult Summary()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)(User.Identity);
            String UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            Cart = new CartViewModel
            {
                Items = unitOfWork.ShopingCartRepository.GetAll(u => u.UserId == UserId, includeProp: "Product"),
                orderHeader = new OrderHeader()
            };
            Cart.orderHeader.applicationUser = unitOfWork.ApplicationUserRepository.Get(i => i.Id == UserId);
            Cart.orderHeader.Name = Cart.orderHeader.applicationUser.Name;
            Cart.orderHeader.City = Cart.orderHeader.applicationUser.City;
            Cart.orderHeader.Street = Cart.orderHeader.applicationUser.Street;
            Cart.orderHeader.State = Cart.orderHeader.applicationUser.State;
            Cart.orderHeader.Postal = Cart.orderHeader.applicationUser.Postal;
            Cart.orderHeader.PhoneNumber = Cart.orderHeader.applicationUser.PhoneNumber;
            foreach (var Item in Cart.Items)
            {
                Item.Price = GetPrice(Item);
                Cart.orderHeader.OrderTotal += Item.Price * Item.Count;
            }
            return View(Cart);
        }

        [HttpPost]
        [ActionName(nameof(Summary))]
        public IActionResult SummaryPost()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
            String UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            Cart.Items = unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == UserId, includeProp: "Product");
            Cart.orderHeader.UserId = UserId;
            ApplicationUser applicationUser = unitOfWork.ApplicationUserRepository.Get(i => i.Id == UserId);
            Cart.orderHeader.OrderDate = DateTime.Now;
            foreach (var Item in Cart.Items)
            {
                Item.Price = GetPrice(Item);
                Cart.orderHeader.OrderTotal += Item.Price * Item.Count;
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                Cart.orderHeader.PaymentStatus = StaticData.PaymentStatusPending;
                Cart.orderHeader.OrderStatus = StaticData.OrderStatusPending;
            }
            else
            {
                Cart.orderHeader.PaymentStatus = StaticData.PaymentStatusDelayedPayment;
                Cart.orderHeader.OrderStatus = StaticData.OrderStatusApproved;
            }
            unitOfWork.OrderHeaderRepository.Add(Cart.orderHeader);
            unitOfWork.Commit();

            foreach (var Item in Cart.Items)
            {
                OrderDetail Detail = new OrderDetail
                {
                    ProductId = Item.ProductId,
                    OrderHeaderId = Cart.orderHeader.Id,
                    Price = Item.Price,
                    Count = Item.Count
                };
                unitOfWork.OrderDetailRepository.Add(Detail);
                unitOfWork.Commit();
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:44327/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation/{Cart.orderHeader.Id}",
                    CancelUrl = domain + $"Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var Item in Cart.Items)
                {
                    SessionLineItemOptions sessionLineItemOptions = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(Item.Price * 100),
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = Item.Product.Title,
                            }
                        },
                        Quantity = Item.Count
                    };
                    options.LineItems.Add(sessionLineItemOptions);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(Cart.orderHeader.Id, session.Id, session.PaymentIntentId);
                unitOfWork.Commit();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = Cart.orderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            var orderHeader = unitOfWork.OrderHeaderRepository.Get(i => i.Id == id, includeProp: "applicationUser");
            if (orderHeader.PaymentStatus != StaticData.PaymentStatusDelayedPayment)
            {// order not made by the company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session != null)
                {
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                        unitOfWork.OrderHeaderRepository.UpdateStatus(id, StaticData.OrderStatusApproved, StaticData.PaymentStatusApproved);
                        unitOfWork.Commit();
                    }
                }
                List<ShoppingCartModel> cart = unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == orderHeader.UserId).ToList();
                unitOfWork.ShopingCartRepository.RemoveRange(cart);
                unitOfWork.Commit();
            }
            return View(id);
        }

        public IActionResult Plus(int CartId)
        {
            var CartItem = unitOfWork.ShopingCartRepository.Get(u => u.CartId == CartId);
            CartItem.Count += 1;
            unitOfWork.ShopingCartRepository.Update(CartItem);
            unitOfWork.Commit();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int CartId)
        {
            var CartItem = unitOfWork.ShopingCartRepository.Get(u => u.CartId == CartId);
            if (CartItem.Count == 1)
            {
                HttpContext.Session.SetInt32(StaticData.SessionCart, unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == CartItem.UserId).Count() - 1);
                unitOfWork.ShopingCartRepository.Remove(CartItem);
            }
            else
            {
                CartItem.Count -= 1;
                unitOfWork.ShopingCartRepository.Update(CartItem);
            }
            unitOfWork.Commit();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int CartId)
        {
            var CartItem = unitOfWork.ShopingCartRepository.Get(u => u.CartId == CartId);
            HttpContext.Session.SetInt32(StaticData.SessionCart, unitOfWork.ShopingCartRepository.GetAll(i => i.UserId == CartItem.UserId).Count() - 1);
            unitOfWork.ShopingCartRepository.Remove(CartItem);
            unitOfWork.Commit();
            return RedirectToAction(nameof(Index));
        }
    }
}

