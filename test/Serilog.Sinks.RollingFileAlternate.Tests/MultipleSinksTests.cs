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
        public void WillLogToSeparateFiles()
        {
            var sink1 = AlternateRollingFileSinkFileSink();
            var sink2 = AlternateRollingFileSinkFileSink();

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

        private static AlternateRollingFileSink AlternateRollingFileSinkFileSink()
        {
            var formatter = new RawFormatter();
            var fileSystem = new FileSystem();

            return new AlternateRollingFileSink(@"c:\temp", formatter, 100000, fileSystem, Encoding.UTF8);
        }
    }
}
