using Catalog.Products;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Infrastructure.Data.EntityConfigurations;

public class ProductEntityTypeConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", CatalogDbContext.DefaultSchema);

        builder.HasKey(c => c.Id);
        builder.HasIndex(x => x.Id).IsUnique();

        builder.Ignore(c => c.DomainEvents);

        builder.Property(ci => ci.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(ci => ci.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(c => c.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId);

        builder.HasOne(c => c.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId);
    }
}