namespace Serilog.Sinks.RollingFileAlternate.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using Sinks.SizeRollingFileSink;

    public class TestDirectory : IDisposable
    {
        private readonly string folder;
        private readonly object _lock = new object();
        private static readonly string SystemTemp = Path.GetTempPath() + "Serilog-SizeRollingFileTests";
        private bool disposed;

        public TestDirectory()
        {
            var subfolderPath = Path.Combine(SystemTemp, Guid.NewGuid().ToString("N"));
            var di = 
                Directory.Exists(subfolderPath)
                    ? new DirectoryInfo(subfolderPath)
                    : Directory.CreateDirectory(subfolderPath);
            this.folder = di.FullName;
        }

        public string LogDirectory { get { return this.folder; } }

        public void CreateLogFile(DateTime date, uint sequence)
        {
            lock (_lock)
            {
                string fileName = Path.Combine(this.folder, new SizeLimitedLogFileInfo(date, sequence, string.Empty).FileName);
                File.Create(fileName).Dispose(); // touch
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (this.disposed) return;
                try
                {
                    Directory.GetFiles(this.folder).ToList().ForEach(File.Delete);
                    Directory.Delete(this.folder);
                }
                finally
                {
                    this.disposed = true;
                }
            }
        }
    }
}