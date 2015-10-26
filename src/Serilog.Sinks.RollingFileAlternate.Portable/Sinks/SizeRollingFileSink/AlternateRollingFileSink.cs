using System;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    /// <summary>
    /// Write log events to a series of files. Each file will be suffixed with a
    /// Date and 5 digit sequence number. No special template in the path specification is
    /// considered.
    /// </summary>
    public sealed class AlternateRollingFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = (typeof (SizeLimitedFileSink).Name);
        private readonly ITextFormatter formatter;
        private readonly long fileSizeLimitBytes;
        private readonly Encoding encoding;
        private SizeLimitedFileSink currentSink;
        private readonly object syncRoot = new object();
        private bool disposed;
        private readonly string logDirectory;
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Construct a <see cref="AlternateRollingFileSink"/>
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="formatter"></param>
        /// <param name="fileSizeLimitBytes">
        /// The size in bytes at which a new file should be created</param>
        /// <param name="fileSystem">Provides access to the file system.</param>
        /// <param name="encoding"></param>
        public AlternateRollingFileSink(
            string logDirectory,
            ITextFormatter formatter,
            long fileSizeLimitBytes,
            IFileSystem fileSystem,
            Encoding encoding = null)
        {
            this.formatter = formatter;
            this.fileSizeLimitBytes = fileSizeLimitBytes;
            this.fileSystem = fileSystem;
            this.encoding = encoding;
            this.logDirectory = logDirectory;
            this.currentSink = GetLatestSink();
        }

        internal SizeLimitedLogFileDescription CurrentLogFile { get { return this.currentSink.LogFileDescription; } }

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

                if (this.currentSink.SizeLimitReached)
                {
                    this.currentSink = NextSizeLimitedFileSink();
                }

                if (this.currentSink != null)
                {
                    this.currentSink.Emit(logEvent);
                }
            }
        }

        private SizeLimitedFileSink GetLatestSink()
        {
            EnsureDirectoryCreated(this.logDirectory);

            LogFileInfo logFileInfo = LogFileInfo.GetLatestOrNew(DateTime.UtcNow, this.logDirectory, fileSystem);

            return new SizeLimitedFileSink(
                this.formatter,
                this.logDirectory,
                new SizeLimitedLogFileDescription(logFileInfo, this.fileSizeLimitBytes),
                this.fileSystem,
                this.encoding);
        }

        private SizeLimitedFileSink NextSizeLimitedFileSink()
        {
            SizeLimitedLogFileDescription next = this.currentSink.LogFileDescription.Next();
            this.currentSink.Dispose();

            return new SizeLimitedFileSink(this.formatter, this.logDirectory, next, this.fileSystem, this.encoding);
        }

        private void EnsureDirectoryCreated(string path)
        {
            if (!fileSystem.DirectoryExists(path))
            {
                fileSystem.CreateDirectory(path);
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
