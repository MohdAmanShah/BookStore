using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface OrderHeaderInterface : RepositoryInterface<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus(int id, String orderStatus, string? PaymentStatus = null);
        void UpdateStripePaymentId(int id, String sessionId, string paymentIntentId);
    }
}
