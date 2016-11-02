using System.IO;
using System.Text;
using Xunit;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileAlternate.Tests.Support;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    public class MultipleSinksTests
    {
        [Fact]
        public void WillLogToSeparateFiles()
        {
            var sink1 = AlternateRollingFileSinkFileSink();
            var sink2 = AlternateRollingFileSinkFileSink();

            var @event = Some.InformationEvent();
            sink1.Emit(@event);
            sink2.Emit(@event);
        }

        private static AlternateRollingFileSink AlternateRollingFileSinkFileSink()
        {
            var formatter = new RawFormatter();

            return new AlternateRollingFileSink(@"c:\temp", formatter, 100000, null, Encoding.UTF8);
        }
    }
}
