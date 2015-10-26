namespace Serilog.Sinks.RollingFileAlternate
{
    /// <summary>
    /// Commonly used Constants.
    /// </summary>
    public class Constants
    {

        /// <summary>
        /// The default output template used for log messages.
        /// </summary>
        public const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";

        /// <summary>
        /// The size of 2 megebytes in bytes.
        /// </summary>
        public const long TwoMegabytes = 1024 * 1024 * 2;

    }
}
