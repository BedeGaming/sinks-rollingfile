namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    using System;
    using System.Collections.Generic;
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
        internal string ProcessName { get; private set; }

        public LogFileInfo(DateTime date, uint sequence)
        {
            var processData = GetProcess();
            this.Date = date;
            this.Sequence = sequence;
            this.ProcessId = processData.Key;
            this.ProcessName = processData.Value;
            this.FileName = String.Format("{0}-{1}-{2}-{3}.log", date.ToString(DateFormat), this.ProcessId, this.ProcessName, sequence.ToString(NumberFormat));
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

        private KeyValuePair<int, string> GetProcess()
        {
            var process = Process.GetCurrentProcess();
            if (process != null)
            {
                var name = process.ProcessName.Length < 20 ? process.ProcessName : process.ProcessName.Substring(0, 20) + "...";
                return new KeyValuePair<int, string>(process.Id, name);
            }
            else
            {
                return new KeyValuePair<int, string>(0, string.Empty);
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
                pattern = string.Format("{0}-{1}-{2}-{3}.log", date.ToString(DateFormat), logFileInfo.ProcessId, logFileInfo.ProcessName, @"(\d{5})");
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
