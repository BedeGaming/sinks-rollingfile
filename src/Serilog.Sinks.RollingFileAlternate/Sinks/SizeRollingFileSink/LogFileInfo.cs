namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    internal class LogFileInfo
    {
        private const string NumberFormat = "00000";
        private const string DateFormat = "yyyyMMdd";

        internal uint Sequence { get; private set; }
        internal string FileName { get; private set; }
        internal DateTime Date { get; private set; }
        internal int ProcessId { get; private set; }

        public LogFileInfo(DateTime date, uint sequence)
        {
            this.Date = date;
            this.Sequence = sequence;
            this.ProcessId = GetProcessId();
            this.FileName = String.Format("{0}-{1}-{2}.log", date.ToString(DateFormat), sequence.ToString(NumberFormat), this.ProcessId);
        }

        public LogFileInfo Next()
        {
            DateTime now = DateTime.UtcNow;
            if (this.Date.Date != now.Date)
            {
                return new LogFileInfo(now, 1);
            }

            return new LogFileInfo(now, this.Sequence + 1);
        }

        private int GetProcessId()
        {
            var process = Process.GetCurrentProcess();
            if (process != null)
            {
                return process.Id;
            }
            else
            {
                return 0;
            }
        }

        internal static LogFileInfo GetLatestOrNew(DateTime date, string logDirectory)
        {
            string pattern;
            var logFileInfo = new LogFileInfo(date, 1);

            if (logFileInfo.ProcessId == 0)
            {
                pattern = date.ToString(DateFormat) + @"-(\d{5}).log";
            }
            else
            {
                pattern = string.Format("{0}-{1}-{2}.log", date.ToString(DateFormat), @"(\d{5})", logFileInfo.ProcessId);
            }


            foreach (var filePath in Directory.GetFiles(logDirectory))
            {
                Match match = Regex.Match(filePath, pattern);
                if (match.Success)
                {
                    var sequence = uint.Parse(match.Groups[1].Value);

                    if (sequence > logFileInfo.Sequence)
                    {
                        logFileInfo = new LogFileInfo(date, sequence);
                    }
                }
            }

            return logFileInfo;
        }
    }
}
