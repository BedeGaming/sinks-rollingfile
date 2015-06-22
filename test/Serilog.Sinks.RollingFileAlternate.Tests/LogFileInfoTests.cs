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
            Assert.That(sut.FileName, Is.EqualTo("20150115-00077.log"));
        }
    }
}