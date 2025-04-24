using System.ComponentModel.DataAnnotations;

namespace Sigmentum.Infrastructure.Persistence.Entities;

public class SymbolEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Symbol { get; set; } = string.Empty;
    
    [Required]
    public bool IsStock { get; set; } = false;
    
    [Required]
    public int WinCount { get; set; } = 0;
    
    [Required]
    public int LossCount { get; set; } = 0;
    
    public ICollection<SignalEntity> Signals { get; set; } = new List<SignalEntity>();
}