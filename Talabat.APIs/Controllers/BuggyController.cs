using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Errors;
using Talabat.Repository.Data;

namespace Talabat.APIs.Controllers
{
    
    public class BuggyController : BaseApiController
    {
        private readonly StoreContext _dbContext;

        public BuggyController(StoreContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("notfound")] // GET: /api/buggy/notfound
        public ActionResult GetNotFoundRequest()
        {
            var product = _dbContext.Products.Find(100);

            if(product is null)
                return NotFound(new ApiResponse(404));

            return Ok(product);
        }

        [HttpGet("servererror")]// GET: /api/buggy/servererror

        public ActionResult GetServerError()
        {
            var product = _dbContext.Products.Find(100);

            var productToReturn = product.ToString(); // Will Throw Exception [Null Reference Exception]

            return Ok(productToReturn);
        }

        [HttpGet("badrequest")]// GET: /api/buggy/badrequest

        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse(400));
        }

        [HttpGet("badrequest/{id}")]// GET: /api/buggy/badrequest/five

        public ActionResult GetValidationError(int id) // Validation Error
        {
            return Ok();
        }

        [HttpGet("unauthorized")] // GET: /api/buggy/unauthorized
        public ActionResult GetUnauthorizedError()
        {
            return Unauthorized(new ApiResponse(401));
        }
    }
}
