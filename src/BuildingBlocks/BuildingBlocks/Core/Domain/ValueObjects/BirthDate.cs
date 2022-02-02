using BuildingBlocks.Core.Domain.Exceptions;
using BuildingBlocks.Core.Domain.Model;

namespace BuildingBlocks.Core.ValueObjects;

// BirthDate
public class BirthDate : ValueObject
{
    public DateTime Value { get; }

    public BirthDate(DateTime value)
    {
        if (value == default)
        {
            throw new DomainException($"BirthDate {value} cannot be null");
        }

        DateTime minDateOfBirth = DateTime.Now.AddYears(-115);
        DateTime maxDateOfBirth = DateTime.Now.AddYears(-15);

        // Validate the minimum age.
        if (value < minDateOfBirth || value > maxDateOfBirth)
        {
            throw new DomainException("The minimum age has to be 15 years.");
        }

        Value = value;
    }

    public static implicit operator BirthDate?(DateTime? value) => value == null ? null : new((DateTime)value);

    public static implicit operator DateTime?(BirthDate? value) => value?.Value ?? null;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}