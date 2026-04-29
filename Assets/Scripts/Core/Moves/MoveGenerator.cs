using System.Collections.Generic;
using Chess.Core.Board;

namespace Chess.Core.Moves
{
    public static class MoveGenerator
    {
        public static void GeneratePseudoLegal(BoardState board, List<Move> moves)
        {
            PieceColor sideToMove = board.SideToMove;
            ulong ownOccupancy = sideToMove == PieceColor.White ? board.WhiteOccupancy : board.BlackOccupancy;
            ulong opponentOccupancy = sideToMove == PieceColor.White ? board.BlackOccupancy : board.WhiteOccupancy;
            ulong allOccupancy = board.AllOccupancy;

            GeneratePawnMoves(board, moves, sideToMove, opponentOccupancy, allOccupancy);
            GenerateKnightMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy);
            GenerateBishopMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy, allOccupancy);
            GenerateRookMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy, allOccupancy);
            GenerateQueenMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy, allOccupancy);
            GenerateKingMoves(board, moves, sideToMove, ownOccupancy, opponentOccupancy);
        }

        private static void GenerateKnightMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy)
        {
            ulong knightBitboard = board.BitboardFor(PieceType.Knight, sideToMove);
            while (knightBitboard != 0)
            {
                int fromSquare = Bitboard.PopLowestSetBitIndex(ref knightBitboard);
                ulong targetBitboard = PrecomputedAttacks.KnightAttacks[fromSquare] & ~ownOccupancy;
                EmitTargets(moves, fromSquare, targetBitboard, opponentOccupancy);
            }
        }

        private static void GenerateKingMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy)
        {
            ulong kingBitboard = board.BitboardFor(PieceType.King, sideToMove);
            if (kingBitboard == 0) return;
            int fromSquare = Bitboard.LowestSetBitIndex(kingBitboard);
            ulong targetBitboard = PrecomputedAttacks.KingAttacks[fromSquare] & ~ownOccupancy;
            EmitTargets(moves, fromSquare, targetBitboard, opponentOccupancy);
        }

        private static void GenerateBishopMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy)
            => GenerateSlidingMoves(board.BitboardFor(PieceType.Bishop, sideToMove), moves, ownOccupancy, opponentOccupancy, allOccupancy, includeBishopRays: true, includeRookRays: false);

        private static void GenerateRookMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy)
            => GenerateSlidingMoves(board.BitboardFor(PieceType.Rook, sideToMove), moves, ownOccupancy, opponentOccupancy, allOccupancy, includeBishopRays: false, includeRookRays: true);

        private static void GenerateQueenMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy)
            => GenerateSlidingMoves(board.BitboardFor(PieceType.Queen, sideToMove), moves, ownOccupancy, opponentOccupancy, allOccupancy, includeBishopRays: true, includeRookRays: true);

        private static void GenerateSlidingMoves(ulong pieceBitboard, List<Move> moves, ulong ownOccupancy, ulong opponentOccupancy, ulong allOccupancy, bool includeBishopRays, bool includeRookRays)
        {
            while (pieceBitboard != 0)
            {
                int fromSquare = Bitboard.PopLowestSetBitIndex(ref pieceBitboard);
                ulong attackBitboard = 0UL;
                if (includeBishopRays) attackBitboard |= SlidingAttacks.Bishop(fromSquare, allOccupancy);
                if (includeRookRays) attackBitboard |= SlidingAttacks.Rook(fromSquare, allOccupancy);
                attackBitboard &= ~ownOccupancy;
                EmitTargets(moves, fromSquare, attackBitboard, opponentOccupancy);
            }
        }

        private static void EmitTargets(List<Move> moves, int fromSquare, ulong targetBitboard, ulong opponentOccupancy)
        {
            while (targetBitboard != 0)
            {
                int toSquare = Bitboard.PopLowestSetBitIndex(ref targetBitboard);
                MoveFlags flags = (opponentOccupancy & (1UL << toSquare)) != 0 ? MoveFlags.Capture : MoveFlags.None;
                moves.Add(new Move(fromSquare, toSquare, flags));
            }
        }

        private static void GeneratePawnMoves(BoardState board, List<Move> moves, PieceColor sideToMove, ulong opponentOccupancy, ulong allOccupancy)
        {
            ulong pawnBitboard = board.BitboardFor(PieceType.Pawn, sideToMove);
            int forwardOffset = sideToMove == PieceColor.White ? 8 : -8;
            int startRank = sideToMove == PieceColor.White ? 1 : 6;
            int promotionRank = sideToMove == PieceColor.White ? 7 : 0;

            while (pawnBitboard != 0)
            {
                int fromSquare = Bitboard.PopLowestSetBitIndex(ref pawnBitboard);
                int oneStepSquare = fromSquare + forwardOffset;
                if (oneStepSquare >= 0 && oneStepSquare < 64 && (allOccupancy & (1UL << oneStepSquare)) == 0)
                {
                    if (Square.RankIndex(oneStepSquare) == promotionRank)
                        AddPromotions(moves, fromSquare, oneStepSquare, MoveFlags.None);
                    else
                        moves.Add(new Move(fromSquare, oneStepSquare, MoveFlags.None));

                    if (Square.RankIndex(fromSquare) == startRank)
                    {
                        int twoStepSquare = oneStepSquare + forwardOffset;
                        if ((allOccupancy & (1UL << twoStepSquare)) == 0)
                            moves.Add(new Move(fromSquare, twoStepSquare, MoveFlags.DoublePawnPush));
                    }
                }

                ulong captureBitboard = PrecomputedAttacks.PawnAttacks[(int)sideToMove, fromSquare] & opponentOccupancy;
                while (captureBitboard != 0)
                {
                    int toSquare = Bitboard.PopLowestSetBitIndex(ref captureBitboard);
                    if (Square.RankIndex(toSquare) == promotionRank)
                        AddPromotions(moves, fromSquare, toSquare, MoveFlags.Capture);
                    else
                        moves.Add(new Move(fromSquare, toSquare, MoveFlags.Capture));
                }

                if (board.EnPassantSquare >= 0)
                {
                    ulong enPassantTargetBit = 1UL << board.EnPassantSquare;
                    if ((PrecomputedAttacks.PawnAttacks[(int)sideToMove, fromSquare] & enPassantTargetBit) != 0)
                        moves.Add(new Move(fromSquare, board.EnPassantSquare, MoveFlags.Capture | MoveFlags.EnPassant));
                }
            }
        }

        private static void AddPromotions(List<Move> moves, int fromSquare, int toSquare, MoveFlags baseFlags)
        {
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Queen));
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Rook));
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Bishop));
            moves.Add(new Move(fromSquare, toSquare, baseFlags | MoveFlags.Promotion, PieceType.Knight));
        }
    }
}
