using System.ComponentModel.DataAnnotations;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.APIs.Dtos
{
    public class OrderDto
    {
        
        [Required]
        public string BasketId { get; set; }
        [Required]
        public int DeliveryMethodId { get; set; }
        public AddressDto shipToAddress { get; set; }
    }
}
