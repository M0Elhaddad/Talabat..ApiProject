using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Service.Contract;
using Talabat.Core.Specifications.Order_Specs;

namespace Talabat.Service
{
    public class OrderService : IOrderService
    {
        private readonly IPaymentService _paymentService;
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;

        ///private readonly IGenericRepository<Product> _productsRepo;
        ///private readonly IGenericRepository<DeliveryMethod> _deliveryMethodsRepo;
        ///private readonly IGenericRepository<Order> _ordersRepo;

        public OrderService(
            IPaymentService paymentService,
            IBasketRepository basketRepo,
            IUnitOfWork unitOfWork
            ///IGenericRepository<Product> productsRepo,
            ///IGenericRepository<DeliveryMethod> deliveryMethodsRepo,
            ///IGenericRepository<Order> ordersRepo
            )
        {
            _paymentService = paymentService;
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;

            ///_productsRepo = productsRepo;
            ///_deliveryMethodsRepo = deliveryMethodsRepo;
            ///_ordersRepo = ordersRepo;
        }
        public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, int deliveryMethodId, Address shippingAddress)
        {
            // 1. Get Basket From Baskets Repo

            var basket = await _basketRepo.GetBasketAsync(basketId);

            // 2. Get Selected Items at Basket From Products Repo

            var orderItems = new List<OrderItem>();

            if (basket?.Items?.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                    var productRepository = _unitOfWork.Repository<Product>();

                    var product = await productRepository.GetByIdAsync(item.Id);

                    var productItemOrdered = new ProductItemOrdered(item.Id, product.Name, product.PictureUrl);

                    var orderItem = new OrderItem(productItemOrdered, product.Price, item.Quantity);

                    orderItems.Add(orderItem);
                }
            }

            // 3. Calculate SubTotal

            var subTotal = orderItems.Sum(orderItem => orderItem.Price * orderItem.Quantity);

            // 4. Get Delivery Method From DeliveryMethods Repo

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            var ordersRepo = _unitOfWork.Repository<Order>();

            var orderSpecs = new OrderWithPaymentIntentSpecifications(basket.PaymentIntentId);

            var existingOrder = await ordersRepo.GetEntityWithSpecAsync(orderSpecs);

            if(existingOrder != null)
            {
                ordersRepo.Delete(existingOrder);

                //await _unitOfWork.CompleteAsync();

                await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            }


            // 5. Create Order

            var order = new Order(buyerEmail, shippingAddress, deliveryMethod, orderItems, subTotal, basket.PaymentIntentId);

            await ordersRepo.AddAsync(order);

            // 6. Save To Database [TODO]

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return null;

            return order;
        }



        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var ordersRepo = _unitOfWork.Repository<Order>();

            var spec = new OrdersSpecifications(buyerEmail);

            var orders = await ordersRepo.GetAllWithSpecAysnc(spec);

            return orders;
        }


        public Task<Order?> GetOrderByIdForUserAsync(int orderId, string buyerEmail)
        {
            //=> await _unitOfWork.Repository<Order>().GetByIdWithSpecAsync(new OrdersSpecifications(orderId, buyerEmail))

            var orderRepo = _unitOfWork.Repository<Order>();

            var orderSpec = new OrdersSpecifications(orderId, buyerEmail);

            var order = orderRepo.GetEntityWithSpecAsync(orderSpec);

            return order;
        }

        public Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            // => await _unitOfWork.Repository<DeliveryMethod>().GetAllAysnc();

            var deliveryMethodsRepo = _unitOfWork.Repository<DeliveryMethod>();

            var deliveryMethods = deliveryMethodsRepo.GetAllAysnc();

            return deliveryMethods;
        }
    }
}
