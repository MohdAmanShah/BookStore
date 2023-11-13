using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class OrderDetailRepo : Repository<OrderDetail>, OrderDetailInterface
    {
        private readonly AppDbContext db;
        public OrderDetailRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(OrderDetail orderDetail)
        {
            db.OrderDetails.Update(orderDetail);
        }
    }
}
