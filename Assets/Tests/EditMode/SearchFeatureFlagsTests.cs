using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class SearchFeatureFlagsTests
    {
        private SearchFeatureFlags _previousFeatures;

        [SetUp]
        public void SaveFeatures()
        {
            _previousFeatures = MinimaxSearch.Features;
            MinimaxSearch.ResetTTStats();
        }

        [TearDown]
        public void RestoreFeatures()
        {
            MinimaxSearch.Features = _previousFeatures;
            MinimaxSearch.ResetTTStats();
        }

        [Test]
        public void FillPercent_IsZero_WhenTranspositionTableDisabled()
        {
            MinimaxSearch.Features = SearchFeatureFlags.UseMoveOrdering;
            var board = FenParser.Parse("6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1");
            MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.AreEqual(0.0, MinimaxSearch.GetTTFillPercent());
        }

        [Test]
        public void FillPercent_IsPositive_AfterSearchWithTranspositionTableEnabled()
        {
            MinimaxSearch.Features = SearchFeatureFlags.All;
            var board = FenParser.Parse("6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1");
            MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.That(MinimaxSearch.GetTTFillPercent(), Is.GreaterThan(0.0));
        }

        [Test]
        public void ResetTTStats_DropsFillPercentToZero()
        {
            MinimaxSearch.Features = SearchFeatureFlags.All;
            var board = FenParser.Parse("6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1");
            MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.That(MinimaxSearch.GetTTFillPercent(), Is.GreaterThan(0.0));
            MinimaxSearch.ResetTTStats();
            Assert.AreEqual(0.0, MinimaxSearch.GetTTFillPercent());
        }

        [Test]
        public void MateInOne_StillFound_WithAllOptimisationsDisabled()
        {
            MinimaxSearch.Features = SearchFeatureFlags.None;
            var board = FenParser.Parse("6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1");
            var result = MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.AreEqual(Square.FromAlgebraic("e8"), result.BestMove.ToSquare);
            Assert.That(result.Score, Is.GreaterThan(MinimaxSearch.MateScore - 1000));
        }

        [Test]
        public void MateInOne_StillFound_WithMoveOrderingOnly()
        {
            MinimaxSearch.Features = SearchFeatureFlags.UseMoveOrdering;
            var board = FenParser.Parse("6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1");
            var result = MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.AreEqual(Square.FromAlgebraic("e8"), result.BestMove.ToSquare);
        }

        [Test]
        public void MateInOne_StillFound_WithTranspositionTableOnly()
        {
            MinimaxSearch.Features = SearchFeatureFlags.UseTranspositionTable;
            var board = FenParser.Parse("6k1/5ppp/8/8/8/8/8/4Q1K1 w - - 0 1");
            var result = MinimaxSearch.FindBestMove(board, depth: 3);
            Assert.AreEqual(Square.FromAlgebraic("e8"), result.BestMove.ToSquare);
        }
    }
}
