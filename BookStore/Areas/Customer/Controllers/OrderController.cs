using DataAccess.Repository;
using DataAccess.Repository.Interfaces;
using DataModels;
using DataModels.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Stripe;
using Stripe.Checkout;
using Stripe.Issuing;
using System.Diagnostics;
using System.Security.Claims;
using Utility;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        [BindProperty]
        public OrderViewModel orderViewModel { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            orderViewModel = new OrderViewModel
            {
                orderHeader = unitOfWork.OrderHeaderRepository.Get(i => i.Id == orderId, includeProp: "applicationUser"),
                orderDetails = unitOfWork.OrderDetailRepository.GetAll(i => i.OrderHeaderId == orderId, includeProp: "Product"),
            };
            return View(orderViewModel);
        }

        [HttpPost]
        [ActionName("Details")]
        public IActionResult Details_Pay_Now()
        {
            orderViewModel.orderHeader = unitOfWork.OrderHeaderRepository.Get(
                i => i.Id == orderViewModel.orderHeader.Id,
                includeProp: "applicationUser");

            orderViewModel.orderDetails = unitOfWork.OrderDetailRepository.GetAll(
                i => i.OrderHeaderId == orderViewModel.orderHeader.Id,
                includeProp: "Product");

            var domain = "https://localhost:44327/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/order/PaymentConfirmation/{orderViewModel.orderHeader.Id}",
                CancelUrl = domain + $"customer/order/Detail?orderId={orderViewModel.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var Item in orderViewModel.orderDetails)
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
            unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderViewModel.orderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Commit();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int id)
        {
            orderViewModel = new OrderViewModel();
            orderViewModel.orderHeader = unitOfWork.OrderHeaderRepository.Get(i => i.Id == id, includeProp: "applicationUser");
            if (orderViewModel.orderHeader.PaymentStatus == StaticData.PaymentStatusDelayedPayment)
            {// order not made by the company
                var service = new SessionService();
                Session session = service.Get(orderViewModel.orderHeader.SessionId);
                if (session != null)
                {
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                        unitOfWork.OrderHeaderRepository.UpdateStatus(id, orderViewModel.orderHeader.OrderStatus, StaticData.PaymentStatusApproved);
                        unitOfWork.Commit();
                    }
                }
            }
            return View(id);
        }

        [HttpPost]
        [Authorize(Roles = StaticData.Admin_Role + "," + StaticData.Emp_Role)]
        public IActionResult UpdateOrderDetails()
        {
            var ObjectFromDb = unitOfWork.OrderHeaderRepository.Get(i => i.Id == orderViewModel.orderHeader.Id);
            ObjectFromDb.Name = orderViewModel.orderHeader.Name;
            ObjectFromDb.PhoneNumber = orderViewModel.orderHeader.PhoneNumber;
            ObjectFromDb.Street = orderViewModel.orderHeader.Street;
            ObjectFromDb.State = orderViewModel.orderHeader.State;
            ObjectFromDb.City = orderViewModel.orderHeader.City;
            ObjectFromDb.Postal = orderViewModel.orderHeader.Postal;
            if (!String.IsNullOrEmpty(orderViewModel.orderHeader.Carrier))
            {
                ObjectFromDb.Carrier = orderViewModel.orderHeader.Carrier;
            }
            if (!String.IsNullOrEmpty(orderViewModel.orderHeader.TrackingNumber))
            {
                ObjectFromDb.TrackingNumber = orderViewModel.orderHeader.TrackingNumber;
            }
            unitOfWork.OrderHeaderRepository.Update(ObjectFromDb);
            unitOfWork.Commit();
            TempData["success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Index), new { orderId = ObjectFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = StaticData.Admin_Role + "," + StaticData.Emp_Role)]
        public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeaderRepository.UpdateStatus(orderViewModel.orderHeader.Id, StaticData.OrderStatusInProcess);
            unitOfWork.Commit();
            TempData["success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = StaticData.Admin_Role + "," + StaticData.Emp_Role)]
        public IActionResult ShipOrder()
        {
            var orderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == orderViewModel.orderHeader.Id);
            orderHeader.TrackingNumber = orderViewModel.orderHeader.TrackingNumber;
            orderHeader.Carrier = orderViewModel.orderHeader.Carrier;
            orderHeader.OrderStatus = StaticData.OrderStatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == StaticData.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            unitOfWork.OrderHeaderRepository.Update(orderHeader);
            unitOfWork.Commit();
            TempData["success"] = "Order Shipped";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.orderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = StaticData.Admin_Role + "," + StaticData.Emp_Role)]
        public IActionResult CancelOrder()
        {
            var orderHeader = unitOfWork.OrderHeaderRepository.Get(u => u.Id == orderViewModel.orderHeader.Id);
            if(orderHeader.PaymentStatus == StaticData.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId,
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, StaticData.OrderStatusCancelled, StaticData.OrderStatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, StaticData.OrderStatusCancelled, StaticData.OrderStatusCancelled);
            }
            unitOfWork.Commit();
            TempData["success"] = "Order Cancelled";
            return RedirectToAction(nameof(Details), new { orderId = orderViewModel.orderHeader.Id });

        }
        #region api call
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> orderheaders;
            if (User.IsInRole(StaticData.Admin_Role) || User.IsInRole(StaticData.Emp_Role))
            {
                orderheaders = unitOfWork.OrderHeaderRepository.GetAll(includeProp: "applicationUser").ToList();
            }
            else
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity)User.Identity;
                String UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderheaders = unitOfWork.OrderHeaderRepository.GetAll(i => i.UserId == UserId, includeProp: "applicationUser").ToList();
            }
            switch (status)
            {
                case "inprocess":
                    orderheaders = orderheaders.Where(i => i.OrderStatus == StaticData.OrderStatusInProcess).ToList();
                    break;
                case "paymentpending":
                    orderheaders = orderheaders.Where(i => i.PaymentStatus == StaticData.PaymentStatusPending).ToList();
                    break;
                case "approved":
                    orderheaders = orderheaders.Where(i => i.OrderStatus == StaticData.OrderStatusApproved).ToList();
                    break;
                case "completed":
                    orderheaders = orderheaders.Where(i => i.OrderStatus == StaticData.OrderStatusShipped).ToList();
                    break;
                default:
                    break;
            }
            return Json(new
            {
                data = orderheaders
            }); ;
        }
        #endregion
    }
}
