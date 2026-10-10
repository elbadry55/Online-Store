using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Online_Store.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Online_Store.OnlineStoreConfigrations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(500);

            builder.Property(r => r.CreatedAt)
                .IsRequired();
        }
    }
}
