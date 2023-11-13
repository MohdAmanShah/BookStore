using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository.Interfaces
{
    public interface IUnitOfWork
    {
        public CategoryInterface CategoryRepository { get; }
        public ProductInterface ProductRepository { get; }
        public CompanyInterface CompanyRepository { get; }

        public ShopingCartInterface ShopingCartRepository { get; }
        public ApplicationUserInterface ApplicationUserRepository { get; }  
        public OrderDetailInterface OrderDetailRepository { get; }
        public OrderHeaderInterface OrderHeaderRepository { get; }
        public ProductImagesInterface ProductImagesRepository { get; }

        void Commit();
    }
}
