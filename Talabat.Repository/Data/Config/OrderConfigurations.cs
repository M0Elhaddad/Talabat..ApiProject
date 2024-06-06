using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Repository.Data.Config
{
    internal class OrderConfigurations : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(O => O.ShippingAddress, ShippingAddress => ShippingAddress.WithOwner()); // 1:1 [Total]

            builder.Property(O => O.Status)
                    .HasConversion(
                            OStatus => OStatus.ToString(),

                            OStatus => (OrderStatus)Enum.Parse(typeof(OrderStatus), OStatus)
                            );

            builder.Property(O => O.Subtotal)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne(o => o.DeliveryMethod)
                   .WithMany()
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(O => O.Items)
                   .WithOne()
                   .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
