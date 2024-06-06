using AutoMapper;
using Talabat.APIs.Dtos;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order_Aggregate;
using UserAddress = Talabat.Core.Entities.Identity.Address;
using OrderAddress = Talabat.Core.Entities.Order_Aggregate.Address;

namespace Talabat.APIs.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.Brand, O => O.MapFrom(s => s.Brand))
                .ForMember(d => d.Category, O => O.MapFrom(s => s.Category))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<ProductPictureUrlResolver>());

            CreateMap<CustomerBasketDto, CustomerBasket>();

            CreateMap<BasketItemDto,BasketItem>();

            CreateMap<AddressDto, OrderAddress>();

            CreateMap<UserAddress, AddressDto>().ReverseMap();

            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, O => O.MapFrom(S => S.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryMethodCost, O => O.MapFrom(S => S.DeliveryMethod.Cost));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.ProductId, O => O.MapFrom(S => S.Product.ProductId))
                .ForMember(d => d.ProductName, O => O.MapFrom(S => S.Product.ProductName))
                .ForMember(d => d.PictureUrl, O => O.MapFrom(S => S.Product.PictureUrl))
                .ForMember(d => d.PictureUrl, O => O.MapFrom<OrderItemPictureUrlResolver>());
                
        }
    }
}
