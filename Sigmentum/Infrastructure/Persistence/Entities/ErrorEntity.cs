using System.ComponentModel.DataAnnotations;

namespace Sigmentum.Infrastructure.Persistence.Entities;

public class ErrorEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Level { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsTest { get; set; } = false;
}