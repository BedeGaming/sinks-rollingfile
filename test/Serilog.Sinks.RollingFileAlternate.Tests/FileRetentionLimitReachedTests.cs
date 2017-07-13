namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    using System.IO;
    using Formatting.Raw;
    using Sinks.SizeRollingFileSink;
    using Support;
    using Xunit;

    public class FileRetentionLimitReachedTests
    {
        [Fact]
        public void ItDeletesTheOldestFiles()
        {
            using (var dir = new TestDirectory())
            {
                using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 1, 3))
                {
                    var logEvent = Some.InformationEvent();
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);

                    Assert.Equal<uint>(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, 5);
                    Assert.Equal(4, Directory.GetFiles(dir.LogDirectory).Length);
                }
            }
        }
        
        [Fact]
        public void ItIgnoresFilesFromOtherSinks()
        {
            using (var dir = new TestDirectory())
            {
                using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 1, 3,logFilePrefix: "Ignore"))
                {
                    var logEvent = Some.InformationEvent();
                    sizeRollingSink.Emit(logEvent);
                }
                
                using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 1, 3))
                {
                    var logEvent = Some.InformationEvent();
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    
                    var files = Directory.GetFiles(dir.LogDirectory);
                    
                    Assert.Equal<uint>(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, 5);
                    Assert.Contains(files, x => x.Substring(x.LastIndexOf("/") +1).StartsWith("Ignore"));
                    Assert.Equal(5, files.Length);
                }
            }
        }
        
        [Fact]
        public void ItIgnoresFilesFromOtherSinksWithPrefix()
        {
            using (var dir = new TestDirectory())
            {
                using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 1, 3,logFilePrefix: "Ignore"))
                {
                    var logEvent = Some.InformationEvent();
                    sizeRollingSink.Emit(logEvent);
                }
                
                using (var sizeRollingSink = new AlternateRollingFileSink(dir.LogDirectory, new RawFormatter(), 1, 3,logFilePrefix: "DoNotIgnore"))
                {
                    var logEvent = Some.InformationEvent();
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    sizeRollingSink.Emit(logEvent);
                    
                    var files = Directory.GetFiles(dir.LogDirectory);
                    
                    Assert.Equal<uint>(sizeRollingSink.CurrentLogFile.LogFileInfo.Sequence, 5);
                    Assert.Contains(files, x => x.Substring(x.LastIndexOf("/") +1).StartsWith("Ignore"));
                    Assert.Equal(5, files.Length);
                }
            }
        }
    }
}