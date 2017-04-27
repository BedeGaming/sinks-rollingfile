using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.SizeRollingFileSink
{
    internal class SizeLimitedLogFileInfo
    {
        private const string NumberFormat = "00000";
        private const string DateFormat = "yyyyMMdd";

        internal uint Sequence { get; private set; }
        internal string FileName { get; private set; }
        internal DateTime Date { get; private set; }

        public SizeLimitedLogFileInfo(DateTime date, uint sequence, string logFilePrefix)
        {
            this.Date = date;
            this.Sequence = sequence;
            this.FileName = $"{logFilePrefix}{date.ToString(DateFormat)}-{sequence.ToString(NumberFormat)}.log";
        }

        public SizeLimitedLogFileInfo Next(string logFilePrefix)
        {
            DateTime now = DateTime.UtcNow;
            if (this.Date.Date != now.Date)
            {
                return new SizeLimitedLogFileInfo(now, 1, logFilePrefix);
            }

            return new SizeLimitedLogFileInfo(now, this.Sequence + 1, logFilePrefix);
        }

        internal static SizeLimitedLogFileInfo GetLatestOrNew(DateTime date, string logDirectory, string logFilePrefix)
        {
            string pattern = $"{logFilePrefix}{date.ToString(DateFormat)}" + @"-(\d{5}).log";

            var logFileInfo = new SizeLimitedLogFileInfo(date, 1, logFilePrefix);

            foreach (var filePath in Directory.GetFiles(logDirectory))
            {
                Match match = Regex.Match(filePath, pattern);
                if (match.Success)
                {
                    var sequence = uint.Parse(match.Groups[1].Value);

                    if (sequence > logFileInfo.Sequence)
                    {
                        logFileInfo = new SizeLimitedLogFileInfo(date, sequence, logFilePrefix);
                    }
                }
            }

            return logFileInfo;
        }
    }
}
