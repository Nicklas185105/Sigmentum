using System.ComponentModel.DataAnnotations;

namespace Sigmentum.Infrastructure.Persistence.Entities;

public class EvaluationResultEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    public string SignalType { get; set; } = string.Empty;

    [Required]
    public string Result { get; set; } = string.Empty; // Win / Loss / Neutral

    [Required]
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsTest { get; set; } = false;
}