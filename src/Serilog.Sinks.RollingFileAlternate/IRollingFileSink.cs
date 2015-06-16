using System;
using Serilog.Core;

namespace Serilog.Sinks.RollingFileAlternate
{
    interface IRollingFileSink : ILogEventSink, IDisposable
    {
    }
}