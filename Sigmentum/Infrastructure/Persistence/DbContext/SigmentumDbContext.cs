using Microsoft.EntityFrameworkCore;
using Sigmentum.Infrastructure.Persistence.Entities;

namespace Sigmentum.Infrastructure.Persistence.DbContext;

public class SigmentumDbContext(DbContextOptions<SigmentumDbContext> options)
    : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<ErrorEntity> Errors => Set<ErrorEntity>();
    public DbSet<EvaluationResultEntity> EvaluationResults => Set<EvaluationResultEntity>();
    public DbSet<SignalEntity> Signals => Set<SignalEntity>();
    public DbSet<ScanEntity> Scans => Set<ScanEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ScanEntity>()
            .Property(e => e.IndicatorsJson)
            .HasColumnType("jsonb");
        
        modelBuilder.Entity<SignalEntity>()
            .HasIndex(e => e.IsPending);

        modelBuilder.Entity<SignalEntity>()
            .HasIndex(e => new { e.Symbol, e.TriggeredAt });
        
        modelBuilder.Entity<SignalEntity>()
            .HasIndex(s => new { s.Symbol, s.Exchange, s.SignalType, s.TriggeredAt })
            .IsUnique();
    }
}