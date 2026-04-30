using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class IterativeDeepeningTests
    {
        [Test]
        public void RespectsTimeBudget()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var deepeningResult = IterativeDeepening.Search(board, maxDepth: 64, timeMs: 200);
            stopwatch.Stop();

            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(500));
            Assert.That(deepeningResult.DepthReached, Is.GreaterThan(0));
            Assert.AreNotEqual(deepeningResult.BestMove.FromSquare, deepeningResult.BestMove.ToSquare);
        }
    }
}
