using System;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling
{
    /// <summary>
    /// Write log events to a series of files. Each file will be suffixed with a
    /// Date and 5 digit sequence number. No special template in the path specification is
    /// considered.
    /// </summary>
    public sealed class HourlyRollingFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = (typeof (HourlyFileSink).Name);
        private readonly ITextFormatter formatter;
        private readonly Encoding encoding;
        private HourlyFileSink currentSink;
        private readonly object syncRoot = new object();
        private bool disposed;
        private readonly string logDirectory;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Construct a <see cref="HourlyRollingFileSink"/>
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="formatter">The size in bytes at which a new file should be created</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <param name="encoding"></param>
        public HourlyRollingFileSink(
            string logDirectory,
            ITextFormatter formatter,
            IFileSystem fileSystem,
            Encoding encoding = null)
        {
            this.formatter = formatter;
            this.fileSystem = fileSystem;
            this.encoding = encoding;
            this.logDirectory = logDirectory;
            this.currentSink = this.GetLatestSink();
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
            return new HourlyFileSink(
                this.formatter,
                this.logDirectory,
                new HourlyLogFileDescription(DateTime.UtcNow),
                fileSystem,
                this.encoding);
        }

        private HourlyFileSink NextFileSink(DateTime dateTimeUtc)
        {
            var next = new HourlyLogFileDescription(dateTimeUtc);
            this.currentSink.Dispose();

            return new HourlyFileSink(this.formatter, this.logDirectory, next, fileSystem, this.encoding);
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
