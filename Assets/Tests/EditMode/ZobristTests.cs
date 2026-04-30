using NUnit.Framework;
using System.Collections.Generic;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class ZobristTests
    {
        [Test]
        public void StartPositionKey_IsStableAndNonZero()
        {
            ulong firstKey = FenParser.Parse(FenParserTests.StartFen).ZobristKey;
            ulong secondKey = FenParser.Parse(FenParserTests.StartFen).ZobristKey;
            Assert.AreEqual(firstKey, secondKey);
            Assert.AreNotEqual(0UL, firstKey);
        }

        [Test]
        public void IncrementalUpdate_MatchesComputeFromScratch_AfterEveryLegalMove()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            foreach (var legalMove in legalMoves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, legalMove);
                ulong incrementalKey = board.ZobristKey;
                ulong fromScratchKey = Zobrist.ComputeFromScratch(board);
                Assert.AreEqual(fromScratchKey, incrementalKey, $"mismatch after {legalMove}");
                MoveExecutor.UnmakeMove(board, legalMove, undoInfo);
            }
        }

        [Test]
        public void UnmakeMove_RestoresOriginalKey()
        {
            var board = FenParser.Parse(FenParserTests.StartFen);
            ulong originalKey = board.ZobristKey;
            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            foreach (var legalMove in legalMoves)
            {
                var undoInfo = MoveExecutor.MakeMove(board, legalMove);
                MoveExecutor.UnmakeMove(board, legalMove, undoInfo);
                Assert.AreEqual(originalKey, board.ZobristKey, $"key not restored after {legalMove}");
            }
        }

        [Test]
        public void DifferentSideToMove_ProducesDifferentKey()
        {
            var whiteToMove = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            var blackToMove = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1");
            Assert.AreNotEqual(whiteToMove.ZobristKey, blackToMove.ZobristKey);
        }

        [Test]
        public void DifferentCastlingRights_ProducesDifferentKey()
        {
            var allRights = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            var noRights = FenParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1");
            Assert.AreNotEqual(allRights.ZobristKey, noRights.ZobristKey);
        }

        [Test]
        public void DifferentEnPassantFile_ProducesDifferentKey()
        {
            var noEpBoard = FenParser.Parse("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq - 0 1");
            var epOnFileE = FenParser.Parse("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1");
            Assert.AreNotEqual(noEpBoard.ZobristKey, epOnFileE.ZobristKey);
        }
    }
}
