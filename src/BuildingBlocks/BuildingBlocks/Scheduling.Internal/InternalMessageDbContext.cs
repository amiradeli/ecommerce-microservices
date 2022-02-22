using BuildingBlocks.Core.Persistence.EfCore;
using BuildingBlocks.Persistence.EfCore.Postgres;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Scheduling.Internal;

public class InternalMessageDbContext : EfDbContextBase
{
    /// <summary>
    /// The default database schema.
    /// </summary>
    public const string DefaultSchema = "messaging";

    public DbSet<InternalMessage> InternalMessages => Set<InternalMessage>();

    public InternalMessageDbContext(DbContextOptions<InternalMessageDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InternalMessageEntityTypeConfiguration());
    }
}
