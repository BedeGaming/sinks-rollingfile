using System;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;

namespace Serilog.Sinks.RollingFileAlternate
{
    /// <summary>
    /// Configuration extensions to be able to use fluent syntax for constructing
    /// a file sink that rolls files based on size.
    /// </summary>
    public static class LoggerConfigurationExtensions
    {

        /// <summary>
        /// Creates an alternative implementation of the rolling file sink
        /// that rolls files based on their size.
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="minimumLevel">Minimum <see cref="LogEventLevel"/></param>
        /// <param name="outputTemplate">The template for substituting logged parameters</param>
        /// <param name="formatProvider">A culture specific format provider</param>
        /// <param name="fileSizeLimitBytes">The size files should grow up to (default 2MB)</param>
        /// <returns></returns>
        public static LoggerConfiguration RollingFileAlternate(
            this LoggerSinkConfiguration configuration,
            IFileSystem fileSystem,
            string logDirectory,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            string outputTemplate = Constants.DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            long? fileSizeLimitBytes = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var templateFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var sink = new AlternateRollingFileSink(logDirectory, templateFormatter, fileSizeLimitBytes ?? Constants.TwoMegabytes, fileSystem);
            return configuration.Sink(sink, minimumLevel);
        }

        /// <summary>
        /// Creates an hourly rolling file sink that rolls files every hour.
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="minimumLevel">Minimum <see cref="LogEventLevel"/></param>
        /// <param name="outputTemplate">The template for substituting logged parameters</param>
        /// <param name="formatProvider">A culture specific format provider</param>
        /// <returns></returns>
        public static LoggerConfiguration HourlyRollingFileAlternate(
            this LoggerSinkConfiguration configuration,
             IFileSystem fileSystem,
            string logDirectory,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            string outputTemplate = Constants.DefaultOutputTemplate,
            IFormatProvider formatProvider = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var templateFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var sink = new HourlyRollingFileSink(logDirectory, templateFormatter, fileSystem);
            return configuration.Sink(sink, minimumLevel);
        }
    }
}
