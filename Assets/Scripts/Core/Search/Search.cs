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
        public readonly bool Aborted;
        public readonly bool HasUsablePartialResult;

        public SearchResult(Move bestMove, int score, long nodesVisited)
            : this(bestMove, score, nodesVisited, aborted: false, hasUsablePartialResult: false)
        {
        }

        public SearchResult(Move bestMove, int score, long nodesVisited, bool aborted, bool hasUsablePartialResult)
        {
            BestMove = bestMove;
            Score = score;
            NodesVisited = nodesVisited;
            Aborted = aborted;
            HasUsablePartialResult = hasUsablePartialResult;
        }
    }

    [System.Flags]
    public enum SearchFeatureFlags : byte
    {
        None = 0,
        UseTranspositionTable = 1,
        UseMoveOrdering = 2,
        UseMagicBitboards = 4,
        All = UseTranspositionTable | UseMoveOrdering | UseMagicBitboards
    }

    public static class MinimaxSearch
    {
        public const int MateScore = 1_000_000;
        public const int Infinity = 10_000_000;
        private const long DeadlineCheckIntervalMask = 1023;

        public static SearchFeatureFlags Features = SearchFeatureFlags.All;

        private static TranspositionTable _transpositionTable = new TranspositionTable(sizeMegabytes: 64);

        public static void ConfigFlags(bool useTranspositionTable, bool useMoveOrdering, bool useMagicBitboards)
        {
            SearchFeatureFlags configuredFlags = SearchFeatureFlags.None;
            if (useTranspositionTable) configuredFlags |= SearchFeatureFlags.UseTranspositionTable;
            if (useMoveOrdering) configuredFlags |= SearchFeatureFlags.UseMoveOrdering;
            if (useMagicBitboards) configuredFlags |= SearchFeatureFlags.UseMagicBitboards;
            Features = configuredFlags;
            SlidingAttacks.UseMagicBitboards = useMagicBitboards;
        }

        public static double GetTTFillPercent()
        {
            if ((Features & SearchFeatureFlags.UseTranspositionTable) == 0) return 0.0;
            return _transpositionTable.FillPercent();
        }

        public static void ResetTTStats() => _transpositionTable.Clear();

        public static SearchResult FindBestMove(BoardState board, int depth)
        {
            return FindBestMove(board, depth, default);
        }

        public static SearchResult FindBestMove(BoardState board, int depth, SearchDeadline deadline)
        {
            if (OpeningBook.Enabled
                && OpeningBook.Default != null
                && board.FullmoveNumber <= OpeningBook.LastFullmoveProbed)
            {
                Move? openingBookMoveOrNull = OpeningBook.Default.PickWeighted(board, OpeningBook.SharedRandom);
                if (openingBookMoveOrNull.HasValue)
                    return new SearchResult(openingBookMoveOrNull.Value, 0, 0);
            }

            long nodesVisited = 0;
            Move bestMoveFromCompletedRootMoves = default;
            int bestScoreFromCompletedRootMoves = -Infinity;
            int rootMovesFullyScored = 0;
            int alpha = -Infinity;
            int beta = Infinity;

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            if (legalMoves.Count == 0)
            {
                int terminalScore = GameStateChecker.InCheck(board) ? -MateScore : 0;
                return new SearchResult(default, terminalScore, 0);
            }

            try
            {
                foreach (var legalMove in legalMoves)
                {
                    var undoInfo = MoveExecutor.MakeMove(board, legalMove);
                    int score;
                    try
                    {
                        score = -AlphaBeta(board, depth - 1, -beta, -alpha, ref nodesVisited, deadline);
                    }
                    finally
                    {
                        MoveExecutor.UnmakeMove(board, legalMove, undoInfo);
                    }

                    rootMovesFullyScored++;
                    if (score > bestScoreFromCompletedRootMoves)
                    {
                        bestScoreFromCompletedRootMoves = score;
                        bestMoveFromCompletedRootMoves = legalMove;
                    }
                    if (score > alpha) alpha = score;
                }
            }
            catch (SearchAbortedException)
            {
                return new SearchResult(
                    bestMove: bestMoveFromCompletedRootMoves,
                    score: bestScoreFromCompletedRootMoves,
                    nodesVisited: nodesVisited,
                    aborted: true,
                    hasUsablePartialResult: rootMovesFullyScored > 0);
            }

            return new SearchResult(bestMoveFromCompletedRootMoves, bestScoreFromCompletedRootMoves, nodesVisited);
        }

        private static int AlphaBeta(BoardState board, int depth, int alpha, int beta, ref long nodesVisited, SearchDeadline deadline)
        {
            nodesVisited++;
            if ((nodesVisited & DeadlineCheckIntervalMask) == 0 && deadline.IsExpired)
                throw new SearchAbortedException();
            if (depth == 0) return Evaluator.Evaluate(board);

            int originalAlpha = alpha;
            bool useTranspositionTable = (Features & SearchFeatureFlags.UseTranspositionTable) != 0;
            bool useMoveOrdering = (Features & SearchFeatureFlags.UseMoveOrdering) != 0;

            Move transpositionBestMove = default;
            if (useTranspositionTable && _transpositionTable.Probe(board.ZobristKey, out var tableEntry))
            {
                transpositionBestMove = tableEntry.BestMove;
                if (tableEntry.Depth >= depth)
                {
                    if (tableEntry.Bound == TranspositionBound.Exact) return tableEntry.Score;
                    if (tableEntry.Bound == TranspositionBound.LowerBound && tableEntry.Score >= beta) return tableEntry.Score;
                    if (tableEntry.Bound == TranspositionBound.UpperBound && tableEntry.Score <= alpha) return tableEntry.Score;
                }
            }

            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            if (legalMoves.Count == 0)
                return GameStateChecker.InCheck(board) ? -MateScore + (1000 - depth) : 0;

            if (useMoveOrdering) OrderMoves(board, legalMoves, transpositionBestMove);

            int bestScore = -Infinity;
            Move bestMoveAtThisNode = default;

            foreach (var legalMove in legalMoves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, legalMove);
                int score;
                try
                {
                    score = -AlphaBeta(board, depth - 1, -beta, -alpha, ref nodesVisited, deadline);
                }
                finally
                {
                    MoveExecutor.UnmakeMove(board, legalMove, undoInfo);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoveAtThisNode = legalMove;
                }
                if (score > alpha) alpha = score;
                if (alpha >= beta) break;
            }

            if (useTranspositionTable)
            {
                TranspositionBound bound;
                if (bestScore <= originalAlpha) bound = TranspositionBound.UpperBound;
                else if (bestScore >= beta) bound = TranspositionBound.LowerBound;
                else bound = TranspositionBound.Exact;
                _transpositionTable.Store(board.ZobristKey, depth, bestScore, bound, bestMoveAtThisNode);
            }

            return bestScore;
        }

        private static void OrderMoves(BoardState board, List<Move> moves, Move transpositionBestMove)
        {
            int ScoreMove(Move candidateMove)
            {
                if (transpositionBestMove.FromSquare == candidateMove.FromSquare
                    && transpositionBestMove.ToSquare == candidateMove.ToSquare
                    && transpositionBestMove.Flags == candidateMove.Flags)
                    return 1_000_000;

                if (candidateMove.IsCapture)
                {
                    PieceType attackerType = board.PieceAt(candidateMove.FromSquare, out _);
                    PieceType victimType = (candidateMove.Flags & MoveFlags.EnPassant) != 0
                        ? PieceType.Pawn
                        : board.PieceAt(candidateMove.ToSquare, out _);
                    return 100_000 + 10 * Evaluator.MaterialValueByPieceType[(int)victimType] - Evaluator.MaterialValueByPieceType[(int)attackerType];
                }

                if (candidateMove.IsPromotion)
                    return 90_000 + Evaluator.MaterialValueByPieceType[(int)candidateMove.PromotionPiece];

                return 0;
            }

            moves.Sort((firstMove, secondMove) => ScoreMove(secondMove).CompareTo(ScoreMove(firstMove)));
        }
    }
}
