using System;
using Xunit;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;

namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    public class LogFileInfoTests
    {
        [Fact]
        public void RendersCorrectlyWithDateAndSequenceNumber()
        {
            var sut = new LogFileInfo(new DateTime(2015, 01, 15), 77);
            Assert.Equal(sut.FileName, "20150115-00077.log");
        }
    }
}