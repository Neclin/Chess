using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class OpeningBookTests
    {
        private const string PositionAfterE2E4Fen = "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1";
        private const string PositionAfterE2E4D7D5Fen = "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2";

        [Test]
        public void PolyglotKey_StartPosition_MatchesCanonicalConstant()
        {
            BoardState startPositionBoard = FenParser.Parse(FenParserTests.StartFen);
            ulong polyglotKey = PolyglotZobrist.Compute(startPositionBoard);
            Assert.AreEqual(0x463b96181691fc9cUL, polyglotKey);
        }

        [Test]
        public void PolyglotKey_AfterE2E4_MatchesCanonicalConstant()
        {
            BoardState boardAfterE2E4 = FenParser.Parse(PositionAfterE2E4Fen);
            ulong polyglotKey = PolyglotZobrist.Compute(boardAfterE2E4);
            Assert.AreEqual(0x823c9b50fd114196UL, polyglotKey);
        }

        [Test]
        public void PolyglotKey_AfterE2E4D7D5_MatchesCanonicalConstant()
        {
            BoardState boardAfterE2E4D7D5 = FenParser.Parse(PositionAfterE2E4D7D5Fen);
            ulong polyglotKey = PolyglotZobrist.Compute(boardAfterE2E4D7D5);
            Assert.AreEqual(0x0756b94461c50fb0UL, polyglotKey);
        }

        [Test]
        public void LoadFromBytes_OneEntryAtStartPosition_ReturnsThatEntry()
        {
            byte[] singleEntryBookBytes = BuildPolyglotBookBytes(new[]
            {
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("e2", "e4"), weight: (ushort)100)
            });

            OpeningBook book = OpeningBook.LoadFromBytes(singleEntryBookBytes);
            BoardState startPositionBoard = FenParser.Parse(FenParserTests.StartFen);
            IReadOnlyList<OpeningBook.Entry> entries = book.MovesFor(startPositionBoard);

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("e2e4", entries[0].Move.ToString());
            Assert.AreEqual((ushort)100, entries[0].Weight);
        }

        [Test]
        public void StartPosition_HasKnownOpeningMoves()
        {
            byte[] multiEntryBookBytes = BuildPolyglotBookBytes(new[]
            {
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("e2", "e4"), weight: (ushort)50),
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("d2", "d4"), weight: (ushort)40),
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("c2", "c4"), weight: (ushort)10)
            });

            OpeningBook book = OpeningBook.LoadFromBytes(multiEntryBookBytes);
            BoardState startPositionBoard = FenParser.Parse(FenParserTests.StartFen);
            IReadOnlyList<OpeningBook.Entry> entries = book.MovesFor(startPositionBoard);

            Assert.That(entries.Count, Is.GreaterThan(0));
            Assert.IsTrue(entries.Any(entry => entry.Move.ToString() == "e2e4"));
        }

        [Test]
        public void PickWeighted_SeededRandomIsDeterministic()
        {
            byte[] bookBytes = BuildPolyglotBookBytes(new[]
            {
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("e2", "e4"), weight: (ushort)50),
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("d2", "d4"), weight: (ushort)50)
            });
            OpeningBook book = OpeningBook.LoadFromBytes(bookBytes);
            BoardState startPositionBoard = FenParser.Parse(FenParserTests.StartFen);

            Move? firstPickWithSeedZero = book.PickWeighted(startPositionBoard, new System.Random(0));
            Move? secondPickWithSeedZero = book.PickWeighted(startPositionBoard, new System.Random(0));

            Assert.IsTrue(firstPickWithSeedZero.HasValue);
            Assert.AreEqual(firstPickWithSeedZero.Value.ToString(), secondPickWithSeedZero.Value.ToString());
        }

        [Test]
        public void PickWeighted_NoEntriesForPosition_ReturnsNull()
        {
            byte[] bookBytes = BuildPolyglotBookBytes(new[]
            {
                (key: 0x463b96181691fc9cUL, rawMove: EncodePolyglotMove("e2", "e4"), weight: (ushort)100)
            });
            OpeningBook book = OpeningBook.LoadFromBytes(bookBytes);
            BoardState boardAfterE2E4 = FenParser.Parse(PositionAfterE2E4Fen);

            Move? bookMoveOrNull = book.PickWeighted(boardAfterE2E4, new System.Random(0));

            Assert.IsFalse(bookMoveOrNull.HasValue);
        }

        [Test]
        public void LoadFromBytes_EmptyArray_ReturnsEmptyBook()
        {
            OpeningBook emptyBook = OpeningBook.LoadFromBytes(System.Array.Empty<byte>());
            BoardState startPositionBoard = FenParser.Parse(FenParserTests.StartFen);

            Assert.AreEqual(0, emptyBook.MovesFor(startPositionBoard).Count);
        }

        [Test]
        public void DecodePolyglotMove_PromotionFlagsSetCorrectly()
        {
            BoardState boardWithWhitePawnReadyToPromote = FenParser.Parse("4k3/4P3/8/8/8/8/8/4K3 w - - 0 1");
            ulong polyglotKeyForPromotionBoard = PolyglotZobrist.Compute(boardWithWhitePawnReadyToPromote);
            ushort promotionRawMove = (ushort)((4 << 12) | (EncodePolyglotMove("e7", "e8") & 0x0FFF));
            byte[] bookBytes = BuildPolyglotBookBytes(new[]
            {
                (key: polyglotKeyForPromotionBoard, rawMove: promotionRawMove, weight: (ushort)1)
            });
            OpeningBook book = OpeningBook.LoadFromBytes(bookBytes);
            IReadOnlyList<OpeningBook.Entry> promotionEntries = book.MovesFor(boardWithWhitePawnReadyToPromote);

            Assert.AreEqual(1, promotionEntries.Count);
            Move decodedPromotionMove = promotionEntries[0].Move;
            Assert.AreEqual("e7e8q", decodedPromotionMove.ToString());
            Assert.IsTrue(decodedPromotionMove.IsPromotion);
            Assert.AreEqual(PieceType.Queen, decodedPromotionMove.PromotionPiece);
        }

        [Test]
        public void DecodePolyglotMove_KingTakesOwnRook_DecodesAsKingsideCastle()
        {
            BoardState boardWithBareKingsideCastleAvailable = FenParser.Parse("4k3/8/8/8/8/8/8/4K2R w K - 0 1");
            ulong polyglotKeyForCastleBoard = PolyglotZobrist.Compute(boardWithBareKingsideCastleAvailable);
            ushort whiteKingsideCastleRawMove = EncodePolyglotMove("e1", "h1");
            byte[] bookBytes = BuildPolyglotBookBytes(new[]
            {
                (key: polyglotKeyForCastleBoard, rawMove: whiteKingsideCastleRawMove, weight: (ushort)1)
            });
            OpeningBook book = OpeningBook.LoadFromBytes(bookBytes);
            IReadOnlyList<OpeningBook.Entry> castleEntries = book.MovesFor(boardWithBareKingsideCastleAvailable);

            Assert.AreEqual(1, castleEntries.Count);
            Move decodedCastleMove = castleEntries[0].Move;
            Assert.AreEqual(Square.FromAlgebraic("e1"), decodedCastleMove.FromSquare);
            Assert.AreEqual(Square.FromAlgebraic("g1"), decodedCastleMove.ToSquare);
            Assert.IsTrue(decodedCastleMove.IsCastle);
            Assert.IsTrue((decodedCastleMove.Flags & MoveFlags.CastleKingside) != 0);
        }

        private static ushort EncodePolyglotMove(string fromAlgebraic, string toAlgebraic)
        {
            int fromSquare = Square.FromAlgebraic(fromAlgebraic);
            int toSquare = Square.FromAlgebraic(toAlgebraic);
            int toFile = Square.FileIndex(toSquare);
            int toRank = Square.RankIndex(toSquare);
            int fromFile = Square.FileIndex(fromSquare);
            int fromRank = Square.RankIndex(fromSquare);
            return (ushort)((fromRank << 9) | (fromFile << 6) | (toRank << 3) | toFile);
        }

        private static byte[] BuildPolyglotBookBytes((ulong key, ushort rawMove, ushort weight)[] entries)
        {
            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            foreach (var (key, rawMove, weight) in entries)
            {
                WriteBigEndianU64(binaryWriter, key);
                WriteBigEndianU16(binaryWriter, rawMove);
                WriteBigEndianU16(binaryWriter, weight);
                WriteBigEndianU32(binaryWriter, 0);
            }
            return memoryStream.ToArray();
        }

        private static void WriteBigEndianU64(BinaryWriter writer, ulong value)
        {
            for (int byteIndex = 7; byteIndex >= 0; byteIndex--)
                writer.Write((byte)((value >> (byteIndex * 8)) & 0xFF));
        }

        private static void WriteBigEndianU32(BinaryWriter writer, uint value)
        {
            for (int byteIndex = 3; byteIndex >= 0; byteIndex--)
                writer.Write((byte)((value >> (byteIndex * 8)) & 0xFF));
        }

        private static void WriteBigEndianU16(BinaryWriter writer, ushort value)
        {
            writer.Write((byte)((value >> 8) & 0xFF));
            writer.Write((byte)(value & 0xFF));
        }
    }
}
