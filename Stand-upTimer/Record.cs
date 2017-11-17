using System;

namespace Stand_upTimer
{
    internal class Record
    {
        public Record(TimeSpan stopwatchElapsed, bool b)
        {
            Elapsed = stopwatchElapsed;
            IsExceeded = b;
        }

        public TimeSpan Elapsed { get; set; }
        public bool IsExceeded { get; set; }
    }
}