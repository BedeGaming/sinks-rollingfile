namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    internal class SizeLimitedLogFileDescription
    {
        public readonly long SizeLimitBytes;
        public readonly FileNameComponents FileNameComponents;

        public SizeLimitedLogFileDescription(FileNameComponents fileNameComponents, long sizeLimitBytes)
        {
            FileNameComponents = fileNameComponents;
            SizeLimitBytes = sizeLimitBytes;
        }

        public string FullName { get { return FileNameComponents.FullName; } }

        internal SizeLimitedLogFileDescription Next()
        {
            var componentsIncremented = new FileNameComponents(
                this.FileNameComponents.Name,
                this.FileNameComponents.Sequence + 1,
                this.FileNameComponents.Extension);

            return new SizeLimitedLogFileDescription(componentsIncremented, this.SizeLimitBytes);
        }
    }
}
