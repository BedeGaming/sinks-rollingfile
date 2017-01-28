using System;

using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling;
using Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate
{
    /// <summary>
    /// Configuration extensions to be able to use fluent syntax for constructing
    /// a file sink that rolls files based on size.
    /// </summary>
    public static class LoggerConfigurationExtensions
    {
        private const string DefaultOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}";

        private const long TwoMegabytes = 1024*1024*2;

        /// <summary>
        /// Creates an alternative implementation of the rolling file sink
        /// that rolls files based on their size.
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="minimumLevel">Minimum <see cref="LogLevel"/></param>
        /// <param name="outputTemplate">The template for substituting logged parameters</param>
        /// <param name="formatProvider">A culture specific format provider</param>
        /// <param name="fileSizeLimitBytes">The size files should grow up to (default 2MB)</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. The default is null which is unlimited.</param>
        /// <returns></returns>
        public static LoggerConfiguration RollingFileAlternate(
            this LoggerSinkConfiguration configuration,
            string logDirectory,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            long? fileSizeLimitBytes = null,
            int? retainedFileCountLimit = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var templateFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var sink = new AlternateRollingFileSink(logDirectory, templateFormatter, fileSizeLimitBytes ?? TwoMegabytes, retainedFileCountLimit);
            return configuration.Sink(sink, minimumLevel);
        }

        /// <summary>
        /// Creates an alternative implementation of the rolling file sink
        /// that rolls files based on their size with an overload to pass log file name prefix
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="logFilePrefix">The prefix for the log file name.</param>
        /// <param name="minimumLevel">Minimum <see cref="LogLevel"/></param>
        /// <param name="outputTemplate">The template for substituting logged parameters</param>
        /// <param name="formatProvider">A culture specific format provider</param>
        /// <param name="fileSizeLimitBytes">The size files should grow up to (default 2MB)</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. The default is null which is unlimited.</param>
        /// <returns></returns>
        public static LoggerConfiguration RollingFileAlternate(
            this LoggerSinkConfiguration configuration,
            string logDirectory, string logFilePrefix,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            long? fileSizeLimitBytes = null,
            int? retainedFileCountLimit = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var templateFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var sink = new AlternateRollingFileSink(logDirectory, templateFormatter, fileSizeLimitBytes ?? TwoMegabytes, retainedFileCountLimit, logFilePrefix: logFilePrefix);
            return configuration.Sink(sink, minimumLevel);
        }

        /// <summary>
        /// Creates an alternative implementation of the rolling file sink with
        /// an overload to pass an ITextFormatter.
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="formatter">Formatter to control how events are rendered into the file. To control
        /// plain text formatting, use the overload that accepts an output template instead.</param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="minimumLevel">Minimum <see cref="LogLevel"/></param>
        /// <param name="fileSizeLimitBytes">The size files should grow up to (default 2MB)</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. The default is null which is unlimited.</param>
        /// <returns></returns>
        public static LoggerConfiguration RollingFileAlternate(
            this LoggerSinkConfiguration configuration,
            ITextFormatter formatter,
            string logDirectory,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            long? fileSizeLimitBytes = null,
            int? retainedFileCountLimit = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            
            var sink = new AlternateRollingFileSink(logDirectory, formatter, fileSizeLimitBytes ?? TwoMegabytes,
                retainedFileCountLimit);
            return configuration.Sink(sink, minimumLevel);
        }

        /// <summary>
        /// Creates an hourly rolling file sink that rolls files every hour.
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="minimumLevel">Minimum <see cref="LogLevel"/></param>
        /// <param name="outputTemplate">The template for substituting logged parameters</param>
        /// <param name="formatProvider">A culture specific format provider</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. The default is null which is unlimited.</param>
        /// <returns></returns>
        public static LoggerConfiguration HourlyRollingFileAlternate(
            this LoggerSinkConfiguration configuration,
            string logDirectory,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            string outputTemplate = DefaultOutputTemplate,
            IFormatProvider formatProvider = null,
            int? retainedFileCountLimit = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var templateFormatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
            var sink = new HourlyRollingFileSink(logDirectory, templateFormatter, retainedFileCountLimit);
            return configuration.Sink(sink, minimumLevel);
        }
        
        /// <summary>
        /// Creates an hourly rolling file sink that rolls files every hour with an
        /// overload to pass ITextFormatter.
        /// </summary>
        /// <param name="configuration"><see cref="LoggerSinkConfiguration"/></param>
        /// <param name="formatter">Formatter to control how events are rendered into the file. To control
        /// plain text formatting, use the overload that accepts an output template instead.</param>
        /// <param name="logDirectory">The names of the directory to be logged</param>
        /// <param name="minimumLevel">Minimum <see cref="LogLevel"/></param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. The default is null which is unlimited.</param>
        /// <returns></returns>
        public static LoggerConfiguration HourlyRollingFileAlternate(
            this LoggerSinkConfiguration configuration,
            ITextFormatter formatter,
            string logDirectory,
            LogEventLevel minimumLevel = LevelAlias.Minimum,
            int? retainedFileCountLimit = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            var sink = new HourlyRollingFileSink(logDirectory, formatter, retainedFileCountLimit);
            return configuration.Sink(sink, minimumLevel);
        }
    }
}
