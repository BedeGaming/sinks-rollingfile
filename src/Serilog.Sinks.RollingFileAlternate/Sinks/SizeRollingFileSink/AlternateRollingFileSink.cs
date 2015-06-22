using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    /// <summary>
    /// Write log events to a series of files. Each file will be suffixed with a
    /// 5 digit sequence number. No special templating in the path specification is
    /// considered.
    /// </summary>
    public sealed class AlternateRollingFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = (typeof (SizeLimitedFileSink).Name);
        private readonly string filePathTemplate;
        private readonly ITextFormatter formatter;
        private readonly long fileSizeLimitBytes;
        private readonly Encoding encoding;
        private SizeLimitedFileSink currentSink;
        private readonly object syncRoot = new object();
        private bool disposed;
        private readonly string folderPath;

        /// <summary>
        /// Construct a <see cref="AlternateRollingFileSink"/>
        /// </summary>
        /// <param name="filePathTemplate"></param>
        /// <param name="formatter"></param>
        /// <param name="fileSizeLimitBytes">
        /// The size in bytes at which a new file should be created</param>
        /// <param name="encoding"></param>
        public AlternateRollingFileSink(
            string filePathTemplate,
            ITextFormatter formatter,
            long fileSizeLimitBytes,
            Encoding encoding = null)
        {
            this.filePathTemplate = filePathTemplate;
            this.formatter = formatter;
            this.fileSizeLimitBytes = fileSizeLimitBytes;
            this.encoding = encoding;
            this.folderPath = Path.GetDirectoryName(filePathTemplate);
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

                this.currentSink = NewFileWhenLimitReached();

                if(this.currentSink != null)
                    this.currentSink.Emit(logEvent);
            }
        }

        private SizeLimitedFileSink GetLatestSink()
        {
            var latestFileDescription = GetLatestFileDescription();
            return new SizeLimitedFileSink(this.formatter, this.folderPath, latestFileDescription, this.encoding);
        }

        internal SizeLimitedLogFileDescription GetLatestFileDescription()
        {
            IEnumerable<SizeLimitedLogFileDescription> existingFiles =
                GetExistingFiles(this.filePathTemplate, this.fileSizeLimitBytes);
            

            return 
                existingFiles.OrderByDescending(x => x.FileNameComponents.Sequence).FirstOrDefault() ??
                ParseRollingLogfile(this.filePathTemplate, this.fileSizeLimitBytes);
        }


        private SizeLimitedFileSink NewFileWhenLimitReached()
        {
            if(this.currentSink.SizeLimitReached)
            {
                SizeLimitedLogFileDescription next = this.currentSink.LogFileDescription.Next();
                this.currentSink.Dispose();

                return new SizeLimitedFileSink(this.formatter, this.folderPath, next,this.encoding);
            }

            return this.currentSink;
        }

        private static IEnumerable<SizeLimitedLogFileDescription> GetExistingFiles(string filePathTemplate, long fileSizeLimitBytes)
        {
            var directoryName = Path.GetDirectoryName(filePathTemplate);
            if (string.IsNullOrEmpty(directoryName))
            {
#if ASPNETCORE50
                directory = Directory.GetCurrentDirectory();
#else
                directoryName = Environment.CurrentDirectory;
#endif
            }

            directoryName = Path.GetFullPath(directoryName);

            return
                Directory.GetFiles(directoryName)
                .Select(logFilePath => ParseRollingLogfile(logFilePath, fileSizeLimitBytes))
                .Where(rollingFile => rollingFile != null);
        }

        private static SizeLimitedLogFileDescription ParseRollingLogfile(string logFilePath, long fileSizeLimitBytes)
        {
            var fileNameComponents = FileNameParser.ParseLogFileName(logFilePath);

            return new SizeLimitedLogFileDescription(fileNameComponents, fileSizeLimitBytes);
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
