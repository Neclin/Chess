using NUnit.Framework;
using Chess.Tools;

namespace Chess.Core.Tests
{
    public class StockfishBridgeTests
    {
        private const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        [Test]
        public void Perft_StartPosition_Depth1_MatchesCanonical()
        {
            using var stockfishBridge = new StockfishBridge();
            long stockfishNodeCount = stockfishBridge.Perft(StartPositionFen, 1);
            Assert.AreEqual(20, stockfishNodeCount);
        }

        [Test]
        public void Perft_StartPosition_Depth4_MatchesCanonical()
        {
            using var stockfishBridge = new StockfishBridge();
            long stockfishNodeCount = stockfishBridge.Perft(StartPositionFen, 4);
            Assert.AreEqual(197281, stockfishNodeCount);
        }

        [Test]
        public void Perft_KiwipetePosition_Depth3_MatchesCanonical()
        {
            const string kiwipeteFen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            using var stockfishBridge = new StockfishBridge();
            long stockfishNodeCount = stockfishBridge.Perft(kiwipeteFen, 3);
            Assert.AreEqual(97862, stockfishNodeCount);
        }

        [Test]
        public void BestMove_StartPosition_ReturnsLegalMove()
        {
            using var stockfishBridge = new StockfishBridge();
            string bestMove = stockfishBridge.BestMove(StartPositionFen, movetimeMilliseconds: 50);
            Assert.IsNotNull(bestMove);
            Assert.AreEqual(4, bestMove.Length);
        }
    }
}
