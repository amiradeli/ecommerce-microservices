using Catalog.Products.Models;
using Catalog.Shared.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Products.Data;

public class ProductImageEntityTypeConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images", CatalogDbContext.DefaultSchema);

        builder.HasKey(c => c.Id);
        builder.HasIndex(x => x.Id).IsUnique();
        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => id)
            .ValueGeneratedNever();
    }
}