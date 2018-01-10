using System;

namespace TimingScope
{
    public class TimingLogEntry
    {
        public string Name { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset FinishedAt { get; set; }
        public long Duration { get; set; }
    }
}