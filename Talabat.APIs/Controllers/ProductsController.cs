using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Service.Contract;
using Talabat.Core.Specifications;
using Talabat.Core.Specifications.Product_Specs;

namespace Talabat.APIs.Controllers
{
    
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;
        ///private readonly IGenericRepository<Product> _productsRepo;
        ///private readonly IGenericRepository<ProductBrand> _brandsRepo;
        ///private readonly IGenericRepository<ProductCategory> _categoriesRepo;
        private readonly IMapper _mapper;

        public ProductsController(
            IProductService productService,
            ///IGenericRepository<Product> productsRepo,
            ///IGenericRepository<ProductBrand> brandsRepo,
            ///IGenericRepository<ProductCategory> categoriesRepo,
            IMapper mapper)
        {
            _productService = productService;
            ///_productsRepo = productsRepo;
            ///_brandsRepo = brandsRepo;
            ///_categoriesRepo = categoriesRepo;
            _mapper = mapper;
        }

        [CachedAttribute(600)] // Action Filter
        //[Authorize/*(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)*/]
        [HttpGet] // GET: /api/products
        public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams specParams)
         {
            

            var products = await _productService.GetProductsAsync(specParams);
            
            var count = await _productService.GetCountAsync(specParams);

            var data = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            return Ok(new Pagination<ProductToReturnDto>(specParams.PageIndex,specParams.PageSize,count,data));
        }

        // /api/Products/1
        [CachedAttribute(600)] // Action Filter
        [ProducesResponseType(typeof(ProductToReturnDto), StatusCodes.Status200OK)]

        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductToReturnDto>> GetProduct (int id)
        {

            var product = await _productService.GetProductAsync(id);

            if(product is null )
                return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<Product,ProductToReturnDto>(product));
        }

        [CachedAttribute(600)] // Action Filter
        [HttpGet("brands")] // GET: /api/products/brands
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
            var brands = await _productService.GetBrandsAsync();

            return Ok(brands);
        }

        [CachedAttribute(600)] // Action Filter
        [HttpGet("categories")] // GET: /api/products/categories
        public async Task<ActionResult<IReadOnlyList<ProductCategory>>> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();

            return Ok(categories);
        }
    }
}
