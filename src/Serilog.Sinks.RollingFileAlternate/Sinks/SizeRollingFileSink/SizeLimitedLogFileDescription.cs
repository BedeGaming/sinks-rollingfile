namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    internal class SizeLimitedLogFileDescription
    {
        public readonly long SizeLimitBytes;
        public readonly SizeLimitedLogFileInfo LogFileInfo;

        public SizeLimitedLogFileDescription(SizeLimitedLogFileInfo logFileInfo, long sizeLimitBytes)
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
