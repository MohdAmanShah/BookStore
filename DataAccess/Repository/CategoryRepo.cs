using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class CategoryRepo : Repository<Category>, CategoryInterface
    {
        private readonly AppDbContext db;
        public CategoryRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(Category category)
        {
            db.Categories.Update(category);
        }
    }
}
