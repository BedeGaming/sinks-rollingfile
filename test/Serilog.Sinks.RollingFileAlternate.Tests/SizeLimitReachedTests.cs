using System;
using System.IO;
using System.Text;
using Xunit;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileAlternate.Tests.Support;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    public class SizeLimitReachedTests
    {
        [Fact]
        public void ReachedWhenAmountOfCharactersWritten()
        {
            var formatter = new RawFormatter();
            var components = new LogFileInfo(new DateTime(2015, 01, 15), 0);
            var logFile = new SizeLimitedLogFileDescription(components, 1);
            using (var str = new MemoryStream())
            using (var wr = new StreamWriter(str, Encoding.UTF8))
            using (var sink = new SizeLimitedFileSink(formatter, logFile, wr))
            {
                var @event = Some.InformationEvent();
                sink.Emit(@event);
                Assert.True(sink.SizeLimitReached);
            }
        }
    }
}