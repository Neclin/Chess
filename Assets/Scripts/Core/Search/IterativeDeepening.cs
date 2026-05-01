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
            var deadline = new SearchDeadline(stopwatch, timeMs);

            Move bestMoveFound = default;
            int bestScoreFound = 0;
            int depthOfBestMove = 0;
            long totalNodesAcrossIterations = 0;

            for (int depth = 1; depth <= maxDepth; depth++)
            {
                SearchResult iterationResult = MinimaxSearch.FindBestMove(board, depth, deadline);
                totalNodesAcrossIterations += iterationResult.NodesVisited;

                if (iterationResult.Aborted)
                {
                    if (iterationResult.HasUsablePartialResult)
                    {
                        bestMoveFound = iterationResult.BestMove;
                        bestScoreFound = iterationResult.Score;
                        depthOfBestMove = depth;
                    }
                    break;
                }

                bestMoveFound = iterationResult.BestMove;
                bestScoreFound = iterationResult.Score;
                depthOfBestMove = depth;

                if (Math.Abs(bestScoreFound) > MinimaxSearch.MateScore - 1000) break;
            }

            return new IterativeDeepeningResult(bestMoveFound, bestScoreFound, depthOfBestMove, totalNodesAcrossIterations);
        }
    }
}
