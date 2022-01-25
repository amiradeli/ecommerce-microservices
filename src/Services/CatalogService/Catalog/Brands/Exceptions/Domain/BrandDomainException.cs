using BuildingBlocks.Core.Domain.Exceptions;

namespace Catalog.Brands.Exceptions.Domain;

public class BrandDomainException : DomainException
{
    public BrandDomainException(string message) : base(message)
    {
    }

    public BrandDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}