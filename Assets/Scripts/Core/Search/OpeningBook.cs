using System.Collections.Generic;
using System.IO;
using Chess.Core.Board;
using Chess.Core.Moves;

namespace Chess.Core.Search
{
    public sealed class OpeningBook
    {
        public readonly struct Entry
        {
            public readonly Move Move;
            public readonly ushort Weight;

            public Entry(Move move, ushort weight)
            {
                Move = move;
                Weight = weight;
            }
        }

        private readonly struct RawEntry
        {
            public readonly ushort RawPolyglotMove;
            public readonly ushort Weight;

            public RawEntry(ushort rawPolyglotMove, ushort weight)
            {
                RawPolyglotMove = rawPolyglotMove;
                Weight = weight;
            }
        }

        private const int PolyglotEntrySizeBytes = 16;

        private static readonly Entry[] EmptyEntryArray = new Entry[0];

        private readonly Dictionary<ulong, List<RawEntry>> _rawEntriesByPolyglotKey = new Dictionary<ulong, List<RawEntry>>();

        public static OpeningBook Default { get; set; }
        public static bool Enabled { get; set; } = true;
        public static System.Random SharedRandom { get; set; } = new System.Random();
        public const int LastFullmoveProbed = 10;

        public int PositionCount => _rawEntriesByPolyglotKey.Count;

        public static OpeningBook LoadFromBytes(byte[] polyglotBookBytes)
        {
            var book = new OpeningBook();
            if (polyglotBookBytes == null || polyglotBookBytes.Length == 0) return book;

            int totalEntries = polyglotBookBytes.Length / PolyglotEntrySizeBytes;
            for (int entryIndex = 0; entryIndex < totalEntries; entryIndex++)
            {
                int byteOffset = entryIndex * PolyglotEntrySizeBytes;
                ulong polyglotKey = ReadBigEndianU64(polyglotBookBytes, byteOffset);
                ushort rawPolyglotMove = ReadBigEndianU16(polyglotBookBytes, byteOffset + 8);
                ushort entryWeight = ReadBigEndianU16(polyglotBookBytes, byteOffset + 10);

                if (entryWeight == 0) continue;

                if (!book._rawEntriesByPolyglotKey.TryGetValue(polyglotKey, out var rawEntryList))
                {
                    rawEntryList = new List<RawEntry>();
                    book._rawEntriesByPolyglotKey[polyglotKey] = rawEntryList;
                }
                rawEntryList.Add(new RawEntry(rawPolyglotMove, entryWeight));
            }
            return book;
        }

        public IReadOnlyList<Entry> MovesFor(BoardState board)
        {
            ulong polyglotKey = PolyglotZobrist.Compute(board);
            if (!_rawEntriesByPolyglotKey.TryGetValue(polyglotKey, out var rawEntryList))
                return EmptyEntryArray;

            var decodedEntries = new Entry[rawEntryList.Count];
            for (int rawEntryIndex = 0; rawEntryIndex < rawEntryList.Count; rawEntryIndex++)
            {
                RawEntry rawEntry = rawEntryList[rawEntryIndex];
                Move decodedMove = DecodePolyglotMove(rawEntry.RawPolyglotMove, board);
                decodedEntries[rawEntryIndex] = new Entry(decodedMove, rawEntry.Weight);
            }
            return decodedEntries;
        }

        public Move? PickWeighted(BoardState board, System.Random rng)
        {
            IReadOnlyList<Entry> entriesForPosition = MovesFor(board);
            if (entriesForPosition.Count == 0) return null;

            int totalWeight = 0;
            for (int entryIndex = 0; entryIndex < entriesForPosition.Count; entryIndex++)
                totalWeight += entriesForPosition[entryIndex].Weight;

            if (totalWeight <= 0) return entriesForPosition[0].Move;

            int weightedRoll = rng.Next(totalWeight);
            int accumulatedWeight = 0;
            for (int entryIndex = 0; entryIndex < entriesForPosition.Count; entryIndex++)
            {
                accumulatedWeight += entriesForPosition[entryIndex].Weight;
                if (weightedRoll < accumulatedWeight) return entriesForPosition[entryIndex].Move;
            }
            return entriesForPosition[0].Move;
        }

        private static Move DecodePolyglotMove(ushort rawPolyglotMove, BoardState board)
        {
            int toFile = rawPolyglotMove & 0b111;
            int toRank = (rawPolyglotMove >> 3) & 0b111;
            int fromFile = (rawPolyglotMove >> 6) & 0b111;
            int fromRank = (rawPolyglotMove >> 9) & 0b111;
            int polyglotPromotionCode = (rawPolyglotMove >> 12) & 0b111;

            int fromSquare = Square.FromFileAndRank(fromFile, fromRank);
            int toSquare = Square.FromFileAndRank(toFile, toRank);

            PieceType movedPieceType = board.PieceAt(fromSquare, out PieceColor movedPieceColor);

            if (movedPieceType == PieceType.King && PolyglotMoveIsKingTakesOwnRook(board, fromSquare, toSquare, movedPieceColor))
            {
                bool isKingsideCastle = Square.FileIndex(toSquare) == 7;
                int castleDestinationSquare = movedPieceColor == PieceColor.White
                    ? (isKingsideCastle ? Square.FromAlgebraic("g1") : Square.FromAlgebraic("c1"))
                    : (isKingsideCastle ? Square.FromAlgebraic("g8") : Square.FromAlgebraic("c8"));
                MoveFlags castleFlag = isKingsideCastle ? MoveFlags.CastleKingside : MoveFlags.CastleQueenside;
                return new Move(fromSquare, castleDestinationSquare, castleFlag);
            }

            MoveFlags moveFlags = MoveFlags.None;
            PieceType promotionPieceType = PolyglotPromotionCodeToPieceType(polyglotPromotionCode);
            if (promotionPieceType != PieceType.None) moveFlags |= MoveFlags.Promotion;

            if (movedPieceType == PieceType.Pawn)
            {
                int rankDifference = toRank - fromRank;
                if (rankDifference == 2 || rankDifference == -2) moveFlags |= MoveFlags.DoublePawnPush;
                if (toSquare == board.EnPassantSquare && fromFile != toFile)
                    moveFlags |= MoveFlags.EnPassant | MoveFlags.Capture;
            }

            ulong opponentOccupancy = movedPieceColor == PieceColor.White ? board.BlackOccupancy : board.WhiteOccupancy;
            if ((opponentOccupancy & (1UL << toSquare)) != 0) moveFlags |= MoveFlags.Capture;

            return new Move(fromSquare, toSquare, moveFlags, promotionPieceType);
        }

        private static bool PolyglotMoveIsKingTakesOwnRook(BoardState board, int kingFromSquare, int rookFromSquare, PieceColor kingColor)
        {
            int homeRank = kingColor == PieceColor.White ? 0 : 7;
            if (Square.RankIndex(kingFromSquare) != homeRank) return false;
            if (Square.RankIndex(rookFromSquare) != homeRank) return false;
            if (Square.FileIndex(kingFromSquare) != 4) return false;
            int targetFile = Square.FileIndex(rookFromSquare);
            if (targetFile != 0 && targetFile != 7) return false;

            ulong sameColorRookBitboard = board.BitboardFor(PieceType.Rook, kingColor);
            return (sameColorRookBitboard & (1UL << rookFromSquare)) != 0;
        }

        private static PieceType PolyglotPromotionCodeToPieceType(int polyglotPromotionCode)
        {
            switch (polyglotPromotionCode)
            {
                case 1: return PieceType.Knight;
                case 2: return PieceType.Bishop;
                case 3: return PieceType.Rook;
                case 4: return PieceType.Queen;
                default: return PieceType.None;
            }
        }

        private static ulong ReadBigEndianU64(byte[] sourceBytes, int byteOffset)
        {
            ulong assembledValue = 0;
            for (int byteIndex = 0; byteIndex < 8; byteIndex++)
                assembledValue = (assembledValue << 8) | sourceBytes[byteOffset + byteIndex];
            return assembledValue;
        }

        private static ushort ReadBigEndianU16(byte[] sourceBytes, int byteOffset)
        {
            return (ushort)((sourceBytes[byteOffset] << 8) | sourceBytes[byteOffset + 1]);
        }
    }
}
