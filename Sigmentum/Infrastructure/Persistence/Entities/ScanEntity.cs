using System.ComponentModel.DataAnnotations;

namespace Sigmentum.Infrastructure.Persistence.Entities;

public class ScanEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Symbol { get; set; } = string.Empty;

    [Required]
    public string Exchange { get; set; } = string.Empty;

    [Required]
    public string IndicatorsJson { get; set; } = string.Empty; // store as JSONB

    [Required]
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}