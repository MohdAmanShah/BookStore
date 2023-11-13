using DataAccess.Repository.Interfaces;
using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class CompanyRepo : Repository<CompanyModel>, CompanyInterface
    {
        private readonly AppDbContext db;
        public CompanyRepo(AppDbContext db) : base(db)
        {
            this.db = db;
        }

        public void Update(CompanyModel company)
        {
            db.Companies.Update(company);
        }
    }
}
