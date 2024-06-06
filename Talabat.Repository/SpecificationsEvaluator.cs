using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;
using Talabat.Core.Specifications;

namespace Talabat.Repository
{
    internal static class SpecificationsEvaluator<TEntity> where TEntity : BaseEntity
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery , ISpecifications<TEntity> spec)
        {
            var query = inputQuery; // _dbContext.set<Product>()

            if(spec.Criteria is not null) // P.Name.ToLower().Contains("mocha") && true && true
                query = query.Where(spec.Criteria);
            // query = _dbContext.set<Product>().Where(P.Name.ToLower().Contains("mocha") && true && true)

            if(spec.OrderBy is not null) // P => P.Name
                query = query.OrderBy(spec.OrderBy);
            // query = _dbContext.set<Product>().Where(P.Name.ToLower().Contains("mocha") && true && true).OrderBy(P => P.Name)

            else if(spec.OrderByDesc  is not null) // P => P.Price
                query = query.OrderByDescending(spec.OrderByDesc);

            if(spec.IsPaginationEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);
            // query = _dbContext.set<Product>().Where(P.Name.ToLower().Contains("mocha") && true && true).OrderBy(P => P.Name).skip(0).Take(5)

            // query =_dbContext.set<Product>().Where(P.Name.ToLower().Contains("mocha") && true && true).OrderBy(P => P.Name).skip(0).Take(5)
            // Includes
            // P => P.Brand
            // P => P.Category

            query = spec.Includes.Aggregate(query, (currentQuery , includeExpression) => currentQuery.Include(includeExpression));

            // _dbContext.set<Product>().Where(P.Name.ToLower().Contains("mocha") && true && true).OrderBy(P => P.Name).skip(0).Take(5).Include(P => P.Brand)
            // _dbContext.set<Product>().Where(P.Name.ToLower().Contains("mocha") && true && true).OrderBy(P => P.Name).skip(0).Take(5).Include(P => P.Brand).Include(P => P.Category)

            return query;
        }
    }
}
