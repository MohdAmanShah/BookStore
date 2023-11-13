using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class ApplicationUserRepo : Repository<ApplicationUser>, ApplicationUserInterface
    {
        private readonly AppDbContext db;
        public ApplicationUserRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }
    }
}
