using System.Globalization;
using System.Text;
using Sigmentum.Models;

namespace Sigmentum.Services;

public class CsvWriter
{
    private const string FILE_PATH = "signals.csv";

    public void SaveSignalsToCsv(List<Signal> signals)
    {
        var sb = new StringBuilder();
        foreach (var signal in signals)
        {
            sb.AppendLine($"{signal.Symbol},{signal.Type},{signal.Reason?.Replace(",", "")},{DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)}");
        }

        File.AppendAllText(FILE_PATH, sb.ToString());
    }
}
