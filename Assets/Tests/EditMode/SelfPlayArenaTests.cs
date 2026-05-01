using NUnit.Framework;
using Chess.Core.Rules;
using Chess.Core.Search;
using Chess.Tools;

namespace Chess.Core.Tests
{
    public class SelfPlayArenaTests
    {
        [TearDown]
        public void RestoreDefaultSearchFlags()
        {
            MinimaxSearch.ConfigFlags(useTranspositionTable: true, useMoveOrdering: true, useMagicBitboards: true);
        }

        [Test, Timeout(60000)]
        public void TwoConfigsCanFinishOneGame()
        {
            var fasterEngineConfig = new EngineConfig { Name = "faster", Depth = 1 };
            var slowerEngineConfig = new EngineConfig { Name = "slower", Depth = 2 };

            var matchResults = SelfPlayArena.RunMatch(slowerEngineConfig, fasterEngineConfig, games: 1);

            Assert.AreEqual(1, matchResults.Count);
            Assert.AreNotEqual(GameResult.Ongoing, matchResults[0].Outcome);
            Assert.Greater(matchResults[0].Plies, 0);
            Assert.AreEqual(matchResults[0].Plies, matchResults[0].Moves.Count);
        }

        [Test]
        public void RunMatchAlternatesColours()
        {
            var firstEngineConfig = new EngineConfig { Name = "first", Depth = 1 };
            var secondEngineConfig = new EngineConfig { Name = "second", Depth = 1 };

            var matchResults = SelfPlayArena.RunMatch(firstEngineConfig, secondEngineConfig, games: 2);

            Assert.AreEqual(2, matchResults.Count);
            Assert.AreEqual("first", matchResults[0].White);
            Assert.AreEqual("second", matchResults[0].Black);
            Assert.AreEqual("second", matchResults[1].White);
            Assert.AreEqual("first", matchResults[1].Black);
        }

        [Test]
        public void SummariseCountsWinsLossesDraws()
        {
            var matchResults = new System.Collections.Generic.List<SelfPlayArena.MatchResult>
            {
                new SelfPlayArena.MatchResult { White = "A", Black = "B", Outcome = GameResult.WhiteWinsByCheckmate },
                new SelfPlayArena.MatchResult { White = "B", Black = "A", Outcome = GameResult.BlackWinsByCheckmate },
                new SelfPlayArena.MatchResult { White = "A", Black = "B", Outcome = GameResult.DrawByStalemate }
            };

            string summary = SelfPlayArena.Summarise(matchResults, aName: "A");

            StringAssert.Contains("2W", summary);
            StringAssert.Contains("0L", summary);
            StringAssert.Contains("1D", summary);
            StringAssert.Contains("3 games", summary);
        }
    }
}
