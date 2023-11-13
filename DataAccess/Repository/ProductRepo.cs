using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ProductRepo : Repository<ProductModel>, ProductInterface
    {
        private readonly AppDbContext db;
        public ProductRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(ProductModel Product)
        {

            var DbObject = db.Product.FirstOrDefault(u => u.ProductId == Product.ProductId);
            DbObject.Title = Product.Title;
            DbObject.Description = Product.Description;
            DbObject.Price50 = Product.Price50;
            DbObject.Price = Product.Price;
            DbObject.ListPrice = Product.ListPrice;
            DbObject.Price100 = Product.Price100;
            DbObject.CategoryId = Product.CategoryId;
            DbObject.Author = Product.Author;
            DbObject.ISBN = Product.ISBN;
            DbObject.Images = Product.Images;
            if (Product.ImageUrl != null && Product.ImageUrl != "Images\\ImagePlaceHolder.svg")
            {
                DbObject.ImageUrl = Product.ImageUrl;
            }
        }

    }
}

