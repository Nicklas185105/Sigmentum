using Sigmentum.Models;

namespace Sigmentum.Services;

public static class SignalCache
{
    public static List<Signal> LatestSignals { get; set; } = new();
}
