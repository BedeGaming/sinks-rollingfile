using System;
using Serilog.Core;

namespace Serilog.Sinks.RollingFileV2
{
    interface IRollingFileSink : ILogEventSink, IDisposable
    {
    }
}