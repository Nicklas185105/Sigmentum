using Microsoft.EntityFrameworkCore;
using Sigmentum.Infrastructure.Persistence.Entities;

namespace Sigmentum.Infrastructure.Persistence.DbContext;

public class LogDbContext(DbContextOptions<LogDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<LogEvent> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LogEvent>().ToTable("logs");
        modelBuilder.Entity<LogEvent>().HasKey(l => l.Id);

        modelBuilder.Entity<LogEvent>().Property(l => l.Id).HasColumnName("id");
        modelBuilder.Entity<LogEvent>().Property(l => l.Message).HasColumnName("message");
        modelBuilder.Entity<LogEvent>().Property(l => l.MessageTemplate).HasColumnName("messagetemplate");
        modelBuilder.Entity<LogEvent>().Property(l => l.Level).HasColumnName("level");
        modelBuilder.Entity<LogEvent>().Property(l => l.TimeStamp).HasColumnName("timestamp");
        modelBuilder.Entity<LogEvent>().Property(l => l.Exception).HasColumnName("exception");
        modelBuilder.Entity<LogEvent>().Property(l => l.LogEventJson).HasColumnName("logevent");
    }
}
