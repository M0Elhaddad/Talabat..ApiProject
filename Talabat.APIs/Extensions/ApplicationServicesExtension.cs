using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Service.Contract;
using Talabat.Repository;
using Talabat.Service;

namespace Talabat.APIs.Extensions
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton(typeof(IResponseCacheService),typeof(ResponseCacheService));

            services.AddScoped(typeof(IPaymentService), typeof(PaymentService));

            services.AddScoped(typeof(IProductService),typeof(ProductService));

            services.AddScoped(typeof(IOrderService), typeof(OrderService));

            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));

            services.AddScoped(typeof(IBasketRepository), typeof(BasketRepository));

            //services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //WebApplicationBuilder.Services.AddAutoMapper(M => M.AddProfile(new MappingProfiles()));

            services.AddAutoMapper(typeof(MappingProfiles));

            services.Configure<ApiBehaviorOptions>(Options =>
            {
                Options.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(p => p.Value.Errors.Count() > 0)
                                                         .SelectMany(P => P.Value.Errors)
                                                         .Select(E => E.ErrorMessage)
                                                         .ToArray();
                    var validationErrorResponse = new ApiValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(validationErrorResponse);
                };
            });

            return services;
        }
    }
}
