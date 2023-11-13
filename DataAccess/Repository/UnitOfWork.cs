using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDbContext _appDbContext;
        public CategoryInterface CategoryRepository { get; private set; }
        public ProductInterface ProductRepository { get; private set; }
        public CompanyInterface CompanyRepository { get; private set; } 
        public ShopingCartInterface ShopingCartRepository { get; private set; }
        public ApplicationUserInterface ApplicationUserRepository { get; private set; }
        public OrderDetailInterface OrderDetailRepository { get; private set; } 
        public OrderHeaderInterface OrderHeaderRepository { get; private set; }
        public ProductImagesInterface ProductImagesRepository { get; private set; }
        public UnitOfWork(AppDbContext db)
        {
            _appDbContext = db;
            CategoryRepository = new CategoryRepo(_appDbContext);
            ProductRepository = new ProductRepo(_appDbContext);
            CompanyRepository = new CompanyRepo(_appDbContext); 
            ShopingCartRepository = new ShopingCartRepo(_appDbContext);
            ApplicationUserRepository = new ApplicationUserRepo(_appDbContext);
            OrderDetailRepository = new OrderDetailRepo(_appDbContext);
            OrderHeaderRepository = new OrderHeaderRepo(_appDbContext);
            ProductImagesRepository = new ProductImageRepo(_appDbContext);
        }
        public void Commit()
        {
            _appDbContext.SaveChanges();
        }
    }
}
