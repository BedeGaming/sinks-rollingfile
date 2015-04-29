namespace Serilog.Sinks.RollingFileV2.Sinks.SizeRollingFileSink
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
    }

    internal static class SizeLimitedLogFileExtensions
    {
        internal static SizeLimitedLogFileDescription Next(this SizeLimitedLogFileDescription previous)
        {
            var componentsIncremented = new FileNameComponents(previous.FileNameComponents.Name,
                previous.FileNameComponents.Sequence + 1, previous.FileNameComponents.Extension);
            return new SizeLimitedLogFileDescription(
                componentsIncremented, previous.SizeLimitBytes);
        }
    }
}
