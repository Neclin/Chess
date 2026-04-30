using System.Collections.Generic;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Rules;

namespace Chess.Core.Search
{
    public readonly struct SearchResult
    {
        public readonly Move BestMove;
        public readonly int Score;
        public readonly long NodesVisited;

        public SearchResult(Move bestMove, int score, long nodesVisited)
        {
            BestMove = bestMove;
            Score = score;
            NodesVisited = nodesVisited;
        }
    }

    public static class MinimaxSearch
    {
        public const int MateScore = 1_000_000;
        public const int Infinity = 10_000_000;

        private static TranspositionTable _transpositionTable = new TranspositionTable(sizeMegabytes: 64);

        public static SearchResult FindBestMove(BoardState board, int depth)
        {
            long nodesVisited = 0;
            Move bestMove = default;
            int bestScore = -Infinity;
            int alpha = -Infinity;
            int beta = Infinity;

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            if (legalMoves.Count == 0)
            {
                int terminalScore = GameStateChecker.InCheck(board) ? -MateScore : 0;
                return new SearchResult(default, terminalScore, 0);
            }

            foreach (var legalMove in legalMoves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, legalMove);
                int score = -AlphaBeta(board, depth - 1, -beta, -alpha, ref nodesVisited);
                MoveExecutor.UnmakeMove(board, legalMove, undoInfo);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = legalMove;
                }
                if (score > alpha) alpha = score;
            }
            return new SearchResult(bestMove, bestScore, nodesVisited);
        }

        private static int AlphaBeta(BoardState board, int depth, int alpha, int beta, ref long nodesVisited)
        {
            nodesVisited++;
            if (depth == 0) return Evaluator.Evaluate(board);

            int originalAlpha = alpha;

            if (_transpositionTable.Probe(board.ZobristKey, out var tableEntry) && tableEntry.Depth >= depth)
            {
                if (tableEntry.Bound == TranspositionBound.Exact) return tableEntry.Score;
                if (tableEntry.Bound == TranspositionBound.LowerBound && tableEntry.Score >= beta) return tableEntry.Score;
                if (tableEntry.Bound == TranspositionBound.UpperBound && tableEntry.Score <= alpha) return tableEntry.Score;
            }

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            if (legalMoves.Count == 0)
                return GameStateChecker.InCheck(board) ? -MateScore + (1000 - depth) : 0;

            int bestScore = -Infinity;
            Move bestMoveAtThisNode = default;

            foreach (var legalMove in legalMoves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, legalMove);
                int score = -AlphaBeta(board, depth - 1, -beta, -alpha, ref nodesVisited);
                MoveExecutor.UnmakeMove(board, legalMove, undoInfo);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoveAtThisNode = legalMove;
                }
                if (score > alpha) alpha = score;
                if (alpha >= beta) break;
            }

            TranspositionBound bound;
            if (bestScore <= originalAlpha) bound = TranspositionBound.UpperBound;
            else if (bestScore >= beta) bound = TranspositionBound.LowerBound;
            else bound = TranspositionBound.Exact;
            _transpositionTable.Store(board.ZobristKey, depth, bestScore, bound, bestMoveAtThisNode);

            return bestScore;
        }
    }
}
