using Sigmentum.Infrastructure.Persistence.Entities;

namespace Sigmentum.Services;

public class ConfigService
{
    public ConfigEntity Config { get; set; } = new();

    public async Task<ConfigEntity> LoadAsync()
    {
        // Simulated data: Load configuration from a database or file
        // In a real-world scenario, this would involve database access or file I/O
        // For this example, we will return a hardcoded configuration
        await Task.Delay(100); // Simulate async operation
        return new ConfigEntity
        {
            RsiThreshold = 30,
            EmaShort = 12,
            EmaLong = 26,
            EnableCrypto = true,
            EnableStocks = false
        };
    }
}