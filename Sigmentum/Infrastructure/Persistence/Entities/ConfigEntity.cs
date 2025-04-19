namespace Sigmentum.Infrastructure.Persistence.Entities;

public class ConfigEntity
{
    public int RsiThreshold { get; set; }
    public int EmaShort { get; set; }
    public int EmaLong { get; set; }
    public bool EnableCrypto { get; set; }
    public bool EnableStocks { get; set; }
}