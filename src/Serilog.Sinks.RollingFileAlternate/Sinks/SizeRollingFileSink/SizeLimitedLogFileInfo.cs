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

        public SizeLimitedLogFileInfo(DateTime date, uint sequence)
        {
            this.Date = date;
            this.Sequence = sequence;
            this.FileName = String.Format("{0}-{1}.log", date.ToString(DateFormat), sequence.ToString(NumberFormat));
        }

        public SizeLimitedLogFileInfo Next()
        {
            DateTime now = DateTime.UtcNow;
            if (this.Date.Date != now.Date)
            {
                return new SizeLimitedLogFileInfo(now, 1);
            }

            return new SizeLimitedLogFileInfo(now, this.Sequence + 1);
        }

        internal static SizeLimitedLogFileInfo GetLatestOrNew(DateTime date, string logDirectory)
        {
            string pattern = date.ToString(DateFormat) + @"-(\d{5}).log";

            var logFileInfo = new SizeLimitedLogFileInfo(date, 1);

            foreach (var filePath in Directory.GetFiles(logDirectory))
            {
                Match match = Regex.Match(filePath, pattern);
                if (match.Success)
                {
                    var sequence = uint.Parse(match.Groups[1].Value);

                    if (sequence > logFileInfo.Sequence)
                    {
                        logFileInfo = new SizeLimitedLogFileInfo(date, sequence);
                    }
                }
            }

            return logFileInfo;
        }
    }
}
