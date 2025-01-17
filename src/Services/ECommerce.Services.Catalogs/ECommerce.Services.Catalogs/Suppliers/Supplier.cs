using MicroBootstrap.Core.Domain.Model;

namespace ECommerce.Services.Catalogs.Suppliers;

public class Supplier : Entity<SupplierId>
{
    public string Name { get; private set; }

    public Supplier(SupplierId id, string name) : base(id)
    {
        Name = name;
    }
}
