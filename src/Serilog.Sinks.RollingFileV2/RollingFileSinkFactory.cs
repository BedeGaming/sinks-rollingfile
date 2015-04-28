using System;
using Serilog.Core;
using Serilog.Formatting.Display;
using Serilog.Sinks.RollingFileV2.Sinks.SizeRollingFileSink;

namespace Serilog.Sinks.RollingFileV2
{
    static class RollingFileSinkFactory
    {
        private const long TwoMegabytes = 1024*1024*2;

        internal static ILogEventSink Construct(string pathFormat, MessageTemplateTextFormatter formatter,
            bool rollOnDate, bool rollOnFileSize, long? fileSizeLimitBytes, int? maxDaysRetainedLimit)
        {
            if (!rollOnDate && !rollOnFileSize)
                throw new Exception(
                    "Must choose either fileSizeLimit or maxDaysRetainedLimit for rolling file sink");

            if (!rollOnDate && rollOnFileSize)
                return new SizeRollingFileSink(pathFormat, formatter, fileSizeLimitBytes ?? TwoMegabytes);

            throw new NotImplementedException("Can't roll on date and size yet");
        }
    }
}
