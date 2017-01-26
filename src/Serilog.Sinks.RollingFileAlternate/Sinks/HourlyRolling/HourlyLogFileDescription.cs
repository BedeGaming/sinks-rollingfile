using System;

namespace Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling
{
    internal class HourlyLogFileDescription
    {
        public readonly HourlyLogFileInfo LogFileInfo;

        internal HourlyLogFileDescription(HourlyLogFileInfo logFileInfo, DateTime dateTime)
        {
            this.LogFileInfo = logFileInfo;
            this.DateTime = dateTime;
        }

        // internal string FileName { get { return string.Format("{0}.log", this.dateTime.ToString("HH")); } }
        public string FileName { get { return LogFileInfo.FileName; } }

        public DateTime DateTime { get; private set; }

        internal bool SameHour(DateTime logEventAt)
        {
            return this.DateTime.Date == logEventAt.Date && this.DateTime.Hour == logEventAt.Hour;
        }

        internal HourlyLogFileDescription Next()
        {
            return new HourlyLogFileDescription(this.LogFileInfo.Next(), DateTime.UtcNow);
        }
    }
}
