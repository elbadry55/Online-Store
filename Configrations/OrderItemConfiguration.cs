using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Online_Store.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Online_Store.OnlineStoreConfigrations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(oi => oi.Discount)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(oi => oi.Quantity)
                .IsRequired();
        }
    }
}
