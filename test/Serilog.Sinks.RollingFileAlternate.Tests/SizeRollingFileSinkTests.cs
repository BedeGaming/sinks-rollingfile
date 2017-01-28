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

        private class TestDirectory : IDisposable
        {
            private readonly string folder;
            private readonly object _lock = new object();
            private static readonly string SystemTemp = Path.GetTempPath() + "Serilog-SizeRollingFileTests";
            private bool disposed;

            public TestDirectory()
            {
                var subfolderPath = Path.Combine(SystemTemp, Guid.NewGuid().ToString("N"));
                var di = 
                    Directory.Exists(subfolderPath)
                        ? new DirectoryInfo(subfolderPath)
                        : Directory.CreateDirectory(subfolderPath);
                this.folder = di.FullName;
            }

            public string LogDirectory { get { return this.folder; } }

            public void CreateLogFile(DateTime date, uint sequence)
            {
                lock (_lock)
                {
                    string fileName = Path.Combine(this.folder, new SizeLimitedLogFileInfo(date, sequence, string.Empty).FileName);
                    File.Create(fileName).Dispose(); // touch
                }
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    if (this.disposed) return;
                    try
                    {
                        Directory.GetFiles(this.folder).ToList().ForEach(File.Delete);
                        Directory.Delete(this.folder);
                    }
                    finally
                    {
                        this.disposed = true;
                    }
                }
            }
        }
    }
}
