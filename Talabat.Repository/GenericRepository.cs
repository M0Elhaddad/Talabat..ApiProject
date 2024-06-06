using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Repository.Data;

namespace Talabat.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreContext _dbContext;

        public GenericRepository(StoreContext dbContext) // Ask CLR For Creating Object from DbContext Implicitly
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<T>> GetAllAysnc()
        {
            //if(typeof(T) == typeof(Product))
            //    return (IEnumerable<T>) await _dbContext.Set<Product>().OrderBy(P => P.Name).Include(P=>P.Brand).Include(P=>P.Category).ToListAsync();

            return await _dbContext.Set<T>().ToListAsync();
        }


        public async Task<T?> GetByIdAsync(int id)
        {
            //if(typeof(T) == typeof(Product))
            //     return await _dbContext.Set<Product>().Where(P=>P.Id == id).Include(P => P.Brand).Include(P => P.Category).FirstOrDefaultAsync() as T;

            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> GetAllWithSpecAysnc(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).ToListAsync();
            // _dbContext.set<Product>().Include(P => P.Brand).Include(P => P.Category).ToListAsync()
            //_dbContext.set<Product>().OrderByDescending(P => P.Price).Include(P => P.Brand).Include(P => P.Category).ToListAsync()
            // _dbContext.set<Product>().Where(P => P.BrandId == 2 && true).OrderBy(P => P.Name).Include(P => P.Brand).Include(P => P.Category).ToListAsync()
            // _dbContext.set<Product>().Where(true && true).OrderBy(P => P.Name).OrderBy(P => P.Name).Skip(5).Take(5).Include(P => P.Brand).Include(P => P.Category).ToListAsync()
        }

        public async Task<T?> GetEntityWithSpecAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).FirstOrDefaultAsync();
        }
        public async Task<int> GetCountAsync(ISpecifications<T> spec)
        {
            return await ApplySpecifications(spec).CountAsync();
        }

        private IQueryable<T> ApplySpecifications(ISpecifications<T> spec)
        {
            return SpecificationsEvaluator<T>.GetQuery(_dbContext.Set<T>(), spec);
        }

        public async Task AddAsync(T entity)
            => await _dbContext.AddAsync(entity);

        public async void Update(T entity)
             =>  _dbContext.Update(entity);

        public void Delete(T entity)
            => _dbContext.Remove(entity);
    }
}
