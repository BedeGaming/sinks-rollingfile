namespace Serilog.Sinks.RollingFileAlternate.Sinks.HourlyRolling
{
    using System;

    internal class HourlyLogFileDescription
    {
        private readonly DateTime dateTime;

        internal HourlyLogFileDescription(DateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        internal string FileName { get { return string.Format("{0}.log", this.dateTime.ToString("HH")); } }

        internal DateTime Date { get { return this.dateTime.Date; } }

        internal bool SameHour(DateTime logEventAt)
        {
            return this.dateTime.Date == logEventAt.Date && this.dateTime.Hour == logEventAt.Hour;
        }
    }
}
