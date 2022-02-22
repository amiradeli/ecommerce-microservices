namespace BuildingBlocks.Abstractions.Domain.Model;

public interface IHaveAudit : IHaveCreator
{
    DateTime? LastModified { get; }
    int? LastModifiedBy { get; }
}
