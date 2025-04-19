using System.Globalization;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class EvaluationLogger
{
    private const string PendingFilePath = "Data/pending_signals.csv";

    public void SavePendingSignal(Signal signal, decimal entryPrice)
    {
        var target = signal.Type == SignalType.Buy
            ? entryPrice * 1.005m
            : entryPrice * 0.995m;

        var timeout = DateTime.UtcNow.AddHours(1).ToString("o", CultureInfo.InvariantCulture);
        var line = $"{signal.Symbol},{signal.Type},{entryPrice.ToString(CultureInfo.InvariantCulture)},{target.ToString(CultureInfo.InvariantCulture)},{timeout},Pending";

        if (!File.Exists(PendingFilePath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PendingFilePath)!);
            File.WriteAllText(PendingFilePath, "Symbol,Type,EntryPrice,TargetPrice,TimeoutUtc,Status\n");
        }

        File.AppendAllText(PendingFilePath, line + "\n");
    }
}