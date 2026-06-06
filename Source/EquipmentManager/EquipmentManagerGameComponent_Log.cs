using System.Collections.Generic;
using JetBrains.Annotations;

namespace EquipmentManager;

internal partial class EquipmentManagerGameComponent
{
    private const int LogLimit = 5000;
    private readonly List<string> _log = new();

    public IReadOnlyList<string> GetLog()
    {
        return _log;
    }

    public void LogMessage([CanBeNull] string message)
    {
        if (_log.Count >= LogLimit) { _log.RemoveAt(0); }
        if (!string.IsNullOrWhiteSpace(message)) { _log.Add(message); }
    }
}