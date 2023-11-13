using DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class Repository<T> : RepositoryInterface<T> where T : class
    {
        private DbSet<T> dbset;
        private readonly AppDbContext db;
        public Repository(AppDbContext db)
        {
            this.db = db;
            dbset = this.db.Set<T>();
        }
        public void Add(T entity)
        {
            dbset.Add(entity);
        }

        public void Remove(T entity)
        {
            dbset.Remove(entity);
        }
        public void RemoveRange(IEnumerable<T> entities)
        {
            dbset.RemoveRange(entities);
        }
        public T Get(Expression<Func<T, bool>> filter, string? includeProp = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbset;
            }
            else
            {
                query = dbset.AsNoTrackingWithIdentityResolution();
            }
            query = query.Where(filter);
            if (!String.IsNullOrEmpty(includeProp))
            {
                foreach (var item in includeProp.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(item);
                }
            }
            T result = query.FirstOrDefault();
            return result;
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProp = null, bool tracked = false)
        {
            IQueryable<T> entities;
            if (tracked)
            {
                entities = dbset;
            }
            else
            {
                entities = dbset.AsNoTrackingWithIdentityResolution();
            }
            if (filter != null)
            {
                entities = entities.Where(filter);
            }
            if (!String.IsNullOrEmpty(includeProp))
            {
                foreach (var item in includeProp.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    entities = entities.Include(item);
                }
            }
            return entities.ToList();
        }
    }
}
