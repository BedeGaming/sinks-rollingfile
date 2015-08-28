using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileAlternate.Tests.Support;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    [TestFixture]
    public class MultipleSinksTests
    {
        [Test]
        public void CanLogToSameFile()
        {
            var sink1 = CreateSizeLimitedFileSink();
            var sink2 = CreateSizeLimitedFileSink();

            var @event = Some.InformationEvent();
            sink1.Emit(@event);
            try
            {
                sink2.Emit(@event);
            }
            catch (IOException ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        private static SizeLimitedFileSink CreateSizeLimitedFileSink()
        {
            var formatter = new RawFormatter();
            var components = new LogFileInfo(new DateTime(2015, 01, 15), 0);
            var logFile = new SizeLimitedLogFileDescription(components, 1);

            return new SizeLimitedFileSink(formatter, @"c:\temp", logFile, Encoding.UTF8);
        }
    }
}
