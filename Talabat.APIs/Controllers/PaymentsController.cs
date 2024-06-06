using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Service.Contract;

namespace Talabat.APIs.Controllers
{
    [Authorize]
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;
        private const string _whSecrect = "whsec_af4a558e28269773e695fd87979a0835562b2fc2f42595b5a7ec228d48c1f0c2";

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [ProducesResponseType(typeof(CustomerBasket),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status400BadRequest)]
        [HttpPost("{basketId}")] // POST: /api/payments/basketId

        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if (basket is null) return BadRequest(new ApiResponse(400, "An Error With Your Basket"));

            return Ok(basket);
        }

        [AllowAnonymous]
        [HttpPost("webhook")] // POST: /api/Payments/webhook
        public async Task<ActionResult> StripeWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json,
                Request.Headers["Stripe-Signature"], _whSecrect);

            var paymentIntent = (PaymentIntent)stripeEvent.Data.Object;

            Order order;
            // Handle the event
            switch (stripeEvent.Type)
            {
                case Events.PaymentIntentSucceeded:
                    order = await _paymentService.UpdatePaymentIntentToSucceededOrFailed(paymentIntent.Id, true);
                    _logger.LogInformation("Payment Is Succeeded Ya Hamada", paymentIntent.Id);
                    break;
                case Events.PaymentIntentPaymentFailed:
                    order = await _paymentService.UpdatePaymentIntentToSucceededOrFailed(paymentIntent.Id, false);
                    _logger.LogInformation("Payment Is Faild Ya Hamada", paymentIntent.Id);
                    break;
            }


            return Ok();


        }
    }
}
