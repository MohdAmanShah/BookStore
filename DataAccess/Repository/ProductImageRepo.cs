using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Repository.Interfaces;
using DataModels;
namespace DataAccess.Repository
{
    public class ProductImageRepo : Repository<ProductImagesModel>, ProductImagesInterface
    {
        private AppDbContext db;
        public ProductImageRepo(AppDbContext db) : base(db) 
        {
            this.db = db;   
        }
        public void Update(ProductImagesModel model)
        {
            db.ProductImages.Update(model);
        }
    }
}
