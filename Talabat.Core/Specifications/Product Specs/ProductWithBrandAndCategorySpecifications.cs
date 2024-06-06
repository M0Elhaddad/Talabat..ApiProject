using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities;

namespace Talabat.Core.Specifications.Product_Specs
{
    public class ProductWithBrandAndCategorySpecifications : BaseSpecifications<Product>
    {
        // This Constructor Will be Used For Creating an Object, That will be used to Get All Products
        public ProductWithBrandAndCategorySpecifications(ProductSpecParams specParams) 
            : base(P =>
                   (string.IsNullOrEmpty(specParams.Search) || P.Name.ToLower().Contains(specParams.Search)) &&
                   (!specParams.BrandId.HasValue || P.BrandId == specParams.BrandId.Value) &&
                   (!specParams.CategoryId.HasValue || P.CategoryId == specParams.CategoryId.Value)
            )
        {
            //AddIncludes();

            Includes.Add(P => P.Brand);
            Includes.Add(P => P.Category);

            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                switch (specParams.Sort)
                {
                    case "priceAsc":
                        //OrderBy = P => P.Price;
                        AddOrderBy(P => P.Price);
                        break;
                    case "priceDesc":
                        //OrderByDesc = P => P.Price;
                        AddOrderByDesc(P => P.Price);
                        break;
                    default:
                        AddOrderBy(P => P.Name);
                        break;

                }
            }

            else
                AddOrderBy(P => P.Name);

            // TotalProduct = 18 ~ 20
            // PageSize     = 5
            // PageIndex    = 2

            ApplyPagination((specParams.PageIndex-1)*specParams.PageSize, specParams.PageSize);
        }


        public ProductWithBrandAndCategorySpecifications(int id) 
            : base(P => P.Id == id)
        {
            //AddIncludes();

            Includes.Add(P => P.Brand);
            Includes.Add(P => P.Category);
        }
        //private void AddIncludes()
        //{
        //    Includes.Add(P => P.Brand);
        //    Includes.Add(P => P.Category);
        //}
    }
}
