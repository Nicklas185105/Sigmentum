using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

namespace Sigmentum.Infrastructure.Persistence.Configurations;

public class UuidColumnWriter() : ColumnWriterBase(NpgsqlTypes.NpgsqlDbType.Uuid)
{
    public override object? GetValue(LogEvent logEvent, IFormatProvider? formatProvider = null)
    {
        return Guid.NewGuid(); // Serilog doesn’t give us an ID, so we generate it here
    }
}