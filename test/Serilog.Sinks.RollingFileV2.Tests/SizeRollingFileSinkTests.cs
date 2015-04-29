using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Serilog.Formatting.Raw;
using Serilog.Sinks.RollingFileV2.Sinks.SizeRollingFileSink;
using Serilog.Sinks.RollingFileV2.Tests.Support;

namespace Serilog.Sinks.RollingFileV2.Tests
{
    [TestFixture]
    public class SizeRollingFileSinkTests
    {
        public class GetLatestFileDescription
        {
            [Test]
            public void SequenceIsZeroWhenNoPreviousFile()
            {
                using (var dir = new TestDirectory())
                using (var sizeRollingSink = new SizeRollingFileSink(dir.PathTemplate, new RawFormatter(), 512))
                {
                    var latest = sizeRollingSink.GetLatestFileDescription();
                    Assert.That(latest.FileNameComponents.Sequence, Is.EqualTo(0));
                }
            }

            [Test]
            public void SequenceIsEqualToTheHighestFileWritten()
            {
                using (var dir = new TestDirectory())
                using (var sizeRollingSink = new SizeRollingFileSink(dir.PathTemplate, new RawFormatter(), 512))
                {
                    dir.CreateLogFile(1);
                    dir.CreateLogFile(2);
                    var latest = sizeRollingSink.GetLatestFileDescription();
                    Assert.That(latest.FileNameComponents.Sequence, Is.EqualTo(2));
                }
            }
        }

        [Test]
        public void ItCreatesNewFileWhenSizeLimitReached()
        {
            using (var dir = new TestDirectory())
            using (var sizeRollingSink = new SizeRollingFileSink(dir.PathTemplate, new RawFormatter(), 10))
            {
                var logEvent = Some.InformationEvent();
                sizeRollingSink.Emit(logEvent);
                Assert.That(sizeRollingSink.CurrentLogFile.FileNameComponents.Sequence, Is.EqualTo(0));
                sizeRollingSink.Emit(logEvent);
                Assert.That(sizeRollingSink.CurrentLogFile.FileNameComponents.Sequence, Is.EqualTo(1));
            }
        }

        private class TestDirectory : IDisposable
        {
            private readonly long _sizeLimit;
            private readonly string _folder;
            private readonly object _lock = new object();
            private const string LogFileName = "applog";
            private const string Extension = "txt";
            private static readonly string SystemTemp = Path.GetTempPath() + "Serilog-SizeRollingFileTests";
            private bool _disposed;

            public TestDirectory(long? sizeLimit = null)
            {
                _sizeLimit = sizeLimit ?? 512L;
                var subfolderPath = Path.Combine(SystemTemp, Guid.NewGuid().ToString("N"));
                var di = 
                    Directory.Exists(subfolderPath)
                        ? new DirectoryInfo(subfolderPath)
                        : Directory.CreateDirectory(subfolderPath);
                _folder = di.FullName;
            }

            public string PathTemplate { get { return Path.Combine(_folder, LogFileName + "." + Extension); } }

            public void CreateLogFile(uint? sequence)
            {
                lock (_lock)
                {
                    var fileDescription = new SizeLimitedLogFileDescription(
                        new FileNameComponents(LogFileName, sequence ?? 0, Extension), _sizeLimit);
                    File.Create(Path.Combine(_folder, fileDescription.FullName)).Dispose(); // touch
                }
            }

            public void Dispose()
            {
                lock (_lock)
                {
                    if (_disposed) return;
                    try
                    {
                        Directory.GetFiles(_folder).ToList().ForEach(File.Delete);
                        Directory.Delete(_folder);
                    }
                    finally
                    {
                        _disposed = true;
                    }
                }
            }
        }
    }
}