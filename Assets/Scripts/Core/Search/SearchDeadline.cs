using System.Diagnostics;

namespace Chess.Core.Search
{
    public readonly struct SearchDeadline
    {
        public readonly Stopwatch ElapsedTimer;
        public readonly long DeadlineMilliseconds;

        public SearchDeadline(Stopwatch elapsedTimer, long deadlineMilliseconds)
        {
            ElapsedTimer = elapsedTimer;
            DeadlineMilliseconds = deadlineMilliseconds;
        }

        public bool IsExpired => ElapsedTimer != null && ElapsedTimer.ElapsedMilliseconds >= DeadlineMilliseconds;
    }
}
