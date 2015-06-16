using System.IO;
using System.Text;
using NUnit.Framework;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileAlternate.Tests.Support;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    [TestFixture]
    public class SizeLimitReachedTests
    {
        [Test]
        public void ReachedWhenAmountOfCharactersWritten()
        {
            var formatter = new RawFormatter();
            var components = new FileNameComponents("applog", 0, "txt");
            var logFile = new SizeLimitedLogFileDescription(components, 1);
            using (var str = new MemoryStream())
            using (var wr = new StreamWriter(str, Encoding.UTF8))
            using (var sink = new SizeLimitedFileSink(formatter, logFile, wr))
            {
                var @event = Some.InformationEvent();
                sink.Emit(@event);
                Assert.That(sink.SizeLimitReached, Is.True);
            }
        }
    }
}