namespace Sigmentum.Infrastructure.Persistence.Entities;

public class LogEvent
{
    public Guid Id { get; set; } // UUID
    public string Message { get; set; }
    public string MessageTemplate { get; set; }
    public string Level { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? Exception { get; set; }
    public string LogEventJson { get; set; } // if you want to access raw JSON
}
