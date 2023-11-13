using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ShopingCartRepo : Repository<ShoppingCartModel>, ShopingCartInterface
    {
        private readonly AppDbContext db;
        public ShopingCartRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(ShoppingCartModel cart)
        {
            db.ShoppingCarts.Update(cart);
        }
    }
}
