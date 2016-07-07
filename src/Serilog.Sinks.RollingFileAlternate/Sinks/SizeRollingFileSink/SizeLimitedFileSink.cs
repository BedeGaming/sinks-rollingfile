using System;
using System.IO;
using System.Text;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    public class SizeLimitedFileSink : ILogEventSink, IDisposable
    {
        private static readonly string ThisObjectName = typeof(SizeLimitedFileSink).Name;

        private readonly ITextFormatter formatter;
        private readonly SizeLimitedLogFileDescription sizeLimitedLogFileDescription;
        private readonly StreamWriter output;
        private readonly object syncRoot = new object();
        private bool disposed;
        private bool sizeLimitReached;

        public SizeLimitedFileSink(
            ITextFormatter formatter,
            string logDirectory,
            SizeLimitedLogFileDescription sizeLimitedLogFileDescription,
            Encoding encoding = null)
        {
            this.formatter = formatter;
            this.sizeLimitedLogFileDescription = sizeLimitedLogFileDescription;
            this.output = OpenFileForWriting(logDirectory, sizeLimitedLogFileDescription, encoding ?? Encoding.UTF8);
        }

        public SizeLimitedFileSink(
            ITextFormatter formatter,
            SizeLimitedLogFileDescription sizeLimitedLogFileDescription,
            StreamWriter writer)
        {
            this.formatter = formatter;
            this.sizeLimitedLogFileDescription = sizeLimitedLogFileDescription;
            this.output = writer;
        }

        private StreamWriter OpenFileForWriting(
            string folderPath,
            SizeLimitedLogFileDescription logFileDescription,
            Encoding encoding)
        {
            EnsureDirectoryCreated(folderPath);
            try
            {
                var fullPath = Path.Combine(folderPath, logFileDescription.FileName);
                var stream = File.Open(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read);

                return new StreamWriter(stream, encoding ?? Encoding.UTF8);
            }
            catch (IOException ex)
            {
                // Unfortuantely the exception doesn't have a code to check so need to check the message instead
                if (!ex.Message.StartsWith("The process cannot access the file"))
                {
                    throw;
                }
            }

            return OpenFileForWriting(folderPath, logFileDescription.Next(), encoding);
        }

        private static void EnsureDirectoryCreated(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            lock (this.syncRoot)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(ThisObjectName, "Cannot write to disposed file");
                }

                if (this.output == null)
                {
                    return; 
                }

                this.formatter.Format(logEvent, this.output);
                this.output.Flush();

                if (this.output.BaseStream.Length > this.sizeLimitedLogFileDescription.SizeLimitBytes)
                {
                    this.sizeLimitReached = true;
                }
            }
        }

        public bool SizeLimitReached { get { return this.sizeLimitReached; } }

        internal SizeLimitedLogFileDescription LogFileDescription
        {
            get
            {
                return this.sizeLimitedLogFileDescription;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.output.Flush();
                this.output.Dispose();
                this.disposed = true;
            }
        }
    }
}
