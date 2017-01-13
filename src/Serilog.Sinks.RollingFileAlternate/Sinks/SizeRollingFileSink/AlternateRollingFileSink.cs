using System;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    using Debugging;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Write log events to a series of files. Each file will be suffixed with a
    /// Date and 5 digit sequence number. No special template in the path specification is
    /// considered.
    /// </summary>
    public class AlternateRollingFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = (typeof (SizeLimitedFileSink).Name);
        private readonly ITextFormatter formatter;
        private readonly long fileSizeLimitBytes;
        private readonly int? retainedFileCountLimit;
        private readonly Encoding encoding;
        private SizeLimitedFileSink currentSink;
        private readonly object syncRoot = new object();
        private bool disposed;
        private readonly string logDirectory;

        /// <summary>
        /// Construct a <see cref="AlternateRollingFileSink"/>
        /// </summary>
        /// <param name="logDirectory"></param>
        /// <param name="formatter"></param>
        /// <param name="fileSizeLimitBytes">
        /// The size in bytes at which a new file should be created</param>
        /// <param name="encoding"></param>
        public AlternateRollingFileSink(
            string logDirectory,
            ITextFormatter formatter,
            long fileSizeLimitBytes,
            int? retainedFileCountLimit = null,
            Encoding encoding = null)
        {
            this.formatter = formatter;
            this.fileSizeLimitBytes = fileSizeLimitBytes;
            this.retainedFileCountLimit = retainedFileCountLimit;
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
                bool newDay = this.currentSink.LogFileDescription.LogFileInfo.Date.Date != DateTime.UtcNow.Date
                if (this.currentSink.SizeLimitReached || newDay)
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

            LogFileInfo logFileInfo = LogFileInfo.GetLatestOrNew(DateTime.UtcNow, this.logDirectory);

            return new SizeLimitedFileSink(
                this.formatter,
                this.logDirectory,
                new SizeLimitedLogFileDescription(logFileInfo, this.fileSizeLimitBytes),
                this.encoding);
        }

        private SizeLimitedFileSink NextSizeLimitedFileSink()
        {
            SizeLimitedLogFileDescription next = this.currentSink.LogFileDescription.Next();
            ApplyRetentionPolicy();
            this.currentSink.Dispose();

            return new SizeLimitedFileSink(this.formatter, this.logDirectory, next, this.encoding);
        }

        private static void EnsureDirectoryCreated(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void ApplyRetentionPolicy()
        {
            if (retainedFileCountLimit == null) return;

            var newestFirst = Directory.GetFiles(this.logDirectory)
                .Select(m => new FileInfo(m))
                .OrderByDescending(m => m.CreationTime)
                .Select(m => m.Name);

            var toRemove = newestFirst
                .Where(n => StringComparer.OrdinalIgnoreCase.Compare(this.currentSink.LogFileDescription.FileName, n) != 0)
                .Skip(this.retainedFileCountLimit.Value - 1)
                .ToList();

            foreach (var obsolete in toRemove)
            {
                var fullPath = Path.Combine(this.logDirectory, obsolete);
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine("Error {0} while removing obsolete file {1}", ex, fullPath);
                }
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
