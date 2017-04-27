namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    internal class SizeLimitedLogFileDescription
    {
        public readonly long SizeLimitBytes;
        public readonly SizeLimitedLogFileInfo LogFileInfo;
        public readonly string LogFilePrefix;

        public SizeLimitedLogFileDescription(SizeLimitedLogFileInfo logFileInfo, long sizeLimitBytes, string logFilePrefix)
        {
            LogFileInfo = logFileInfo;
            SizeLimitBytes = sizeLimitBytes;
            LogFilePrefix = logFilePrefix;
        }

        public string FileName { get { return LogFileInfo.FileName; } }

        internal SizeLimitedLogFileDescription Next()
        {
            return new SizeLimitedLogFileDescription(this.LogFileInfo.Next(LogFilePrefix), this.SizeLimitBytes, LogFilePrefix);
        }
    }
}
