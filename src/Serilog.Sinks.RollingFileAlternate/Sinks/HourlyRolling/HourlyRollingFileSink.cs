using System;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Debugging;
using System.IO;
using System.Linq;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling
{
    /// <summary>
    /// Write log events to a series of files. Each file will be suffixed with a
    /// Date and 5 digit sequence number. No special template in the path specification is
    /// considered.
    /// </summary>
    public class HourlyRollingFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = (typeof(HourlyFileSink).Name);
        private readonly ITextFormatter formatter;
        private readonly int? retainedFileCountLimit;
        private readonly Encoding encoding;
        private HourlyFileSink currentSink;
        private readonly object syncRoot = new object();
        private bool disposed;
        private readonly string logDirectory;
        private readonly FileRetentionPolicy fileRetentionPolicy;

        /// <summary>
        /// Construct a <see cref="HourlyRollingFileSink"/>
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="formatter">The size in bytes at which a new file should be created</param>
        /// <param name="retainedFileCountLimit">The maximum number of log files that will be retained,
        /// including the current log file. The default is null which is unlimited.</param>
        /// <param name="encoding"></param>
        public HourlyRollingFileSink(
            string logDirectory,
            ITextFormatter formatter,
            int? retainedFileCountLimit = null,
            Encoding encoding = null)
        {
            this.formatter = formatter;
            this.retainedFileCountLimit = retainedFileCountLimit;
            this.encoding = encoding;
            this.logDirectory = logDirectory;
            this.currentSink = this.GetLatestSink();
            this.fileRetentionPolicy = new FileRetentionPolicy(logDirectory, retainedFileCountLimit);
        }

        /// <summary>
        /// Emits a log event to this sink
        /// </summary>
        /// <param name="logEvent">The <see cref="LogEvent"/> to emit</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Emit(LogEvent logEvent)
        {

            if (logEvent == null)
            {
                throw new ArgumentNullException("logEvent");
            }

            lock (this.syncRoot)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(ThisObjectName, "The rolling file sink has been disposed");
                }

                if (!this.currentSink.LogFileDescription.SameHour(logEvent.Timestamp.UtcDateTime))
                {
                    this.currentSink = this.NextFileSink(logEvent.Timestamp.UtcDateTime);
                }

                if (this.currentSink != null)
                {
                    this.currentSink.Emit(logEvent);
                }
            }
        }

        private HourlyFileSink GetLatestSink()
        {
            EnsureDirectoryCreated(this.logDirectory);

            HourlyLogFileInfo logFileInfo = HourlyLogFileInfo.GetLatestOrNew(DateTime.UtcNow, this.logDirectory);

            return new HourlyFileSink(
                this.formatter,
                this.logDirectory,
                new HourlyLogFileDescription(logFileInfo, DateTime.UtcNow),
                this.encoding);
        }

        private HourlyFileSink NextFileSink(DateTime dateTimeUtc)
        {
            HourlyLogFileDescription next = this.currentSink.LogFileDescription.Next();

            this.fileRetentionPolicy.Apply(this.currentSink.LogFileDescription.FileName);
            this.currentSink.Dispose();

            return new HourlyFileSink(this.formatter, this.logDirectory, next, this.encoding);
        }

        private static void EnsureDirectoryCreated(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or 
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (this.syncRoot)
            {
                if (!this.disposed && this.currentSink != null)
                {
                    this.currentSink.Dispose();
                    this.currentSink = null;
                    this.disposed = true;
                }
            }
        }
    }
}
