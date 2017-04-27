using System;
using System.IO;
using System.Text;

using Serilog.Events;
using Serilog.Formatting;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling
{
    internal class HourlyFileSink : IDisposable
    {
        private static readonly string ThisObjectName = typeof(HourlyFileSink).Name;

        private readonly ITextFormatter formatter;
        private readonly HourlyLogFileDescription hourlyLogFileDescription;
        private readonly StreamWriter output;
        private readonly object syncRoot = new object();
        private bool disposed;
        private bool exceptionAlreadyThrown;

        internal HourlyFileSink(
            ITextFormatter formatter,
            string logRootDirectory,
            HourlyLogFileDescription hourlyLogFileDescription,
            Encoding encoding = null)
        {
            this.formatter = formatter;
            this.hourlyLogFileDescription = hourlyLogFileDescription;

            string logDir = Path.Combine(logRootDirectory, DateTime.UtcNow.ToString("yyyy-MM-dd"));

            this.output = this.OpenFileForWriting(logDir, hourlyLogFileDescription, encoding ?? Encoding.UTF8);
        }

        internal HourlyLogFileDescription LogFileDescription
        {
            get
            {
                return this.hourlyLogFileDescription;
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

        internal void Emit(LogEvent logEvent)
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
            }
        }

        private StreamWriter OpenFileForWriting(
            string folderPath,
            HourlyLogFileDescription logFileDescription,
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
            catch (UnauthorizedAccessException)
            {
                if (exceptionAlreadyThrown)
                {
                    throw;
                }

                exceptionAlreadyThrown = true;
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
    }
}
