using System;

namespace TimingScope
{
    public class TimingLogEntry
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public long Duration { get; set; }
    }
}