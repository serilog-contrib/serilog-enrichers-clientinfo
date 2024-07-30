using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Serilog.Enrichers.ClientInfo.Tests;

public class DelegatingSink(Action<LogEvent> write) : ILogEventSink
{
    private static readonly List<LogEvent> LogsEvents = new();
    private readonly Action<LogEvent> _write = write ?? throw new ArgumentNullException(nameof(write));

    public static IReadOnlyList<LogEvent> Logs => LogsEvents;

    public void Emit(LogEvent logEvent)
    {
        LogsEvents.Add(logEvent);
        _write(logEvent);
    }
}