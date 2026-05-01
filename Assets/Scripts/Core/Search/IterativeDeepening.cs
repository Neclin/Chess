using System;
using System.Diagnostics;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Search
{
    public readonly struct IterativeDeepeningResult
    {
        public readonly Move BestMove;
        public readonly int Score;
        public readonly int DepthReached;
        public readonly long TotalNodes;

        public IterativeDeepeningResult(Move bestMove, int score, int depthReached, long totalNodes)
        {
            BestMove = bestMove;
            Score = score;
            DepthReached = depthReached;
            TotalNodes = totalNodes;
        }
    }

    public static class IterativeDeepening
    {
        public static IterativeDeepeningResult Search(BoardState board, int maxDepth, int timeMs)
        {
            if (OpeningBook.Enabled
                && OpeningBook.Default != null
                && board.FullmoveNumber <= OpeningBook.LastFullmoveProbed)
            {
                Move? openingBookMoveOrNull = OpeningBook.Default.PickWeighted(board, OpeningBook.SharedRandom);
                if (openingBookMoveOrNull.HasValue)
                    return new IterativeDeepeningResult(openingBookMoveOrNull.Value, 0, 0, 0);
            }

            var stopwatch = Stopwatch.StartNew();
            Move bestMove = default;
            int bestScore = 0;
            long totalNodes = 0;
            int depthReached = 0;

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                if (depth > 1 && stopwatch.ElapsedMilliseconds > timeMs / 2) break;

                SearchResult iterationResult = MinimaxSearch.FindBestMove(board, depth);
                bestMove = iterationResult.BestMove;
                bestScore = iterationResult.Score;
                totalNodes += iterationResult.NodesVisited;
                depthReached = depth;

                if (stopwatch.ElapsedMilliseconds > timeMs) break;
                if (Math.Abs(bestScore) > MinimaxSearch.MateScore - 1000) break;
            }

            return new IterativeDeepeningResult(bestMove, bestScore, depthReached, totalNodes);
        }
    }
}
