using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class OrderHeaderRepo : Repository<OrderHeader>, OrderHeaderInterface
    {
        private readonly AppDbContext db;
        public OrderHeaderRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? PaymentStatus = null)
        {
            var orderFromDb = db.OrderHeaders.FirstOrDefault(i => i.Id == id);
            if (orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!String.IsNullOrEmpty(PaymentStatus))
                {
                    orderFromDb.PaymentStatus = PaymentStatus;
                }
            }
        }

        public void UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
        {
            var orderHeader = db.OrderHeaders.FirstOrDefault(i=>i.Id == id);
            if(!String.IsNullOrEmpty(sessionId))
            {
                orderHeader.SessionId = sessionId;
            }
            if (!String.IsNullOrEmpty(paymentIntentId))
            {
                orderHeader.PaymentIntentId = paymentIntentId;
                orderHeader.PaymentDate = DateTime.Now;
            }
        }
    }
}
