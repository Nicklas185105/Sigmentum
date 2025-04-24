using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sigmentum.Infrastructure.Persistence.Entities;

public class SignalEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public Guid SymbolId { get; set; }

    [ForeignKey(nameof(SymbolId))]
    public SymbolEntity? Symbol { get; set; } = default!;
    
    [Required]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    public string SignalType { get; set; } = string.Empty;
    
    [Required]
    public string Indicator { get; set; } = string.Empty;

    public decimal? SignalValue { get; set; }
    
    [Required]
    public bool IsPending { get; set; } = true;
    
    [Required]
    public decimal EntryPrice { get; set; }
    
    [Required]
    public string StrategyVersion { get; set; } = string.Empty;
    
    [Required]
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsTest { get; set; } = false;
}