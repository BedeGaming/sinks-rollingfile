using NUnit.Framework;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    using System;

    [TestFixture]
    public class LogFileInfoTests
    {
        [Test]
        public void RendersCorrectlyWithDateAndSequenceNumber()
        {
            var sut = new LogFileInfo(new DateTime(2015, 01, 15), 77);
            Assert.That(sut.FileName, Is.EqualTo(string.Format("{0}-{1}.{2}", "20150115-00077", sut.ProcessId, "log")));
        }

        [Test]
        public void FileContainsProcessId()
        {       
            var logInfo = new LogFileInfo(new DateTime(2016, 01, 05), 1);
            Assert.That(logInfo.FileName.Contains(logInfo.ProcessId.ToString()));
        }
    }
}