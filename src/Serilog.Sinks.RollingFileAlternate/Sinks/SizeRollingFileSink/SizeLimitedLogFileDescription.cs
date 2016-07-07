namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    public class SizeLimitedLogFileDescription
    {
        public readonly long SizeLimitBytes;
        public readonly LogFileInfo LogFileInfo;

        public SizeLimitedLogFileDescription(LogFileInfo logFileInfo, long sizeLimitBytes)
        {
            LogFileInfo = logFileInfo;
            SizeLimitBytes = sizeLimitBytes;
        }

        public string FileName { get { return LogFileInfo.FileName; } }

        internal SizeLimitedLogFileDescription Next()
        {
            return new SizeLimitedLogFileDescription(this.LogFileInfo.Next(), this.SizeLimitBytes);
        }
    }
}
