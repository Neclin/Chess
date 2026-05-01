using System.Diagnostics;
using NUnit.Framework;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class SearchDeadlineTests
    {
        [Test]
        public void DefaultDeadline_NeverExpires()
        {
            SearchDeadline neverExpires = default;
            Assert.IsFalse(neverExpires.IsExpired);
        }

        [Test]
        public void Deadline_ExpiresWhenStopwatchPassesBudget()
        {
            var elapsedTimer = Stopwatch.StartNew();
            var oneMillisecondDeadline = new SearchDeadline(elapsedTimer, deadlineMilliseconds: 1);
            System.Threading.Thread.Sleep(15);
            Assert.IsTrue(oneMillisecondDeadline.IsExpired);
        }

        [Test]
        public void Deadline_NotExpiredBeforeBudget()
        {
            var elapsedTimer = Stopwatch.StartNew();
            var generousDeadline = new SearchDeadline(elapsedTimer, deadlineMilliseconds: 60_000);
            Assert.IsFalse(generousDeadline.IsExpired);
        }
    }
}
