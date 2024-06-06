using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Service.Contract;

namespace Talabat.APIs.Controllers
{
    [Authorize]
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;

        public OrdersController(IOrderService orderService, IMapper mapper)
        {
            _orderService = orderService;
            _mapper = mapper;
        }

        [ProducesResponseType(typeof(OrderToReturnDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status400BadRequest)]
        [HttpPost] // POST: /api/Orders
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var address = _mapper.Map<AddressDto, Address>(orderDto.shipToAddress);

            var order = await _orderService.CreateOrderAsync(buyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, address);

            if (order is null) return BadRequest(new ApiResponse(400));

            return Ok(_mapper.Map<Order, OrderToReturnDto>(order));
        }

        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpGet] // GET: /api/orders 
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var orders = await _orderService.GetOrdersForUserAsync(buyerEmail);

            return Ok(_mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders));
        }

        [HttpGet("{id}")] // GET: /api/orders/1?ahmed.nasr@linkdev.com
        public async Task<ActionResult<OrderToReturnDto>> GetOrderForUser(int id)
        {
            var buyerEmail = User.FindFirstValue(ClaimTypes.Email);

            var order = await _orderService.GetOrderByIdForUserAsync(id,buyerEmail);

            if (order is null) return NotFound(new ApiResponse(404));

            return Ok(_mapper.Map<OrderToReturnDto>(order));
        }

        [HttpGet("deliveryMethods")] // GET : /api/orders/deliveryMethods
        
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            var deliveryMethods = await _orderService.GetDeliveryMethodsAsync();

            return Ok(deliveryMethods);
        }
    }
}
