using System;
using System.IO;
using System.Linq;
using Xunit;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileAlternate.Tests.Support;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    public class SizeRollingFileSinkTests
    {
        public class GetLatestLogFileInfoOrNew
        {
            [Fact]
            public void SequenceIsOneWhenNoPreviousFile()
            {
                using (var dir = new TestDirectory())
                {
                    var latest = SizeLimitedLogFileInfo.GetLatestOrNew(new DateTime(2015, 01, 15), dir.LogDirectory, string.Empty);
                    Assert.Equal<uint>(latest.Sequence, 1);
                }
            }

            [Fact]
            public void SequenceIsEqualToTheHighestFileWritten()
            {
                var date = new DateTime(2015, 01, 15);
                using (var dir = new TestDirectory())
                {
                    dir.CreateLogFile(date, 1);
                    dir.CreateLogFile(date, 2);
                    dir.CreateLogFile(date, 3);
                    var latest = SizeLimitedLogFileInfo.GetLatestOrNew(new DateTime(2015, 01, 15), dir.LogDirectory, string.Empty);
                    Assert.Equal<uint>(latest.Sequence, 3);
                }
            }
        }

        [Fact]
        public void ItCreatesNewFileWhenSizeLimitReached()
        {
            using (var dir = new TestDirectory())
            using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 10))
            {
                var logEvent = Some.InformationEvent();
                sizeRollingSink.Emit(logEvent);
                Assert.Equal<uint>(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, 1);
                sizeRollingSink.Emit(logEvent);
                Assert.Equal<uint>(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, 2);
            }
        }
    }
}
