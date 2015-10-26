using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileAlternate.Tests.Support;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    [TestFixture]
    public class SizeRollingFileSinkTests
    {
        public class GetLatestLogFileInfoOrNew
        {
            [Test]
            public void SequenceIsOneWhenNoPreviousFile()
            {
                var fileSystem = new FileSystem();
                using (var dir = new TestDirectory())
                {
                    var latest = LogFileInfo.GetLatestOrNew(new DateTime(2015, 01, 15), dir.LogDirectory, fileSystem);
                    Assert.That(latest.Sequence, Is.EqualTo(1));
                }
            }

            [Test]
            public void SequenceIsEqualToTheHighestFileWritten()
            {
                var date = new DateTime(2015, 01, 15);
                var fileSystem = new FileSystem();
                using (var dir = new TestDirectory())
                {
                    dir.CreateLogFile(date, 1);
                    dir.CreateLogFile(date, 2);
                    dir.CreateLogFile(date, 3);
                    var latest = LogFileInfo.GetLatestOrNew(new DateTime(2015, 01, 15), dir.LogDirectory, fileSystem);
                    Assert.That(latest.Sequence, Is.EqualTo(3));
                }
            }
        }

        [Test]
        public void ItCreatesNewFileWhenSizeLimitReached()
        {
            var fileSystem = new FileSystem();
            using (var dir = new TestDirectory())
            using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 10, fileSystem))
            {
                var logEvent = Some.InformationEvent();
                sizeRollingSink.Emit(logEvent);
                Assert.That(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, Is.EqualTo(1));
                sizeRollingSink.Emit(logEvent);
                Assert.That(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, Is.EqualTo(2));
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
                    string fileName = Path.Combine(this.folder, new LogFileInfo(date, sequence).FileName);
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
