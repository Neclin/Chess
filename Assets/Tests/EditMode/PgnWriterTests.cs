using System.Collections.Generic;
using NUnit.Framework;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;
using Chess.Core.Rules;

namespace Chess.Core.Tests
{
    public class PgnWriterTests
    {
        [Test]
        public void HeadersAreWrittenWithSevenMandatoryTagsInStandardOrder()
        {
            var startBoard = FenParser.Parse(FenParserTests.StartFen);
            var headers = new PgnHeaders
            {
                Event = "Casual",
                Site = "Local",
                Date = "1970.01.01",
                Round = "1",
                White = "Player",
                Black = "Engine v0"
            };
            var pgn = PgnWriter.Write(startBoard, new List<Move>(), GameResult.Ongoing, headers);
            StringAssert.Contains("[Event \"Casual\"]", pgn);
            StringAssert.Contains("[Site \"Local\"]", pgn);
            StringAssert.Contains("[Date \"1970.01.01\"]", pgn);
            StringAssert.Contains("[Round \"1\"]", pgn);
            StringAssert.Contains("[White \"Player\"]", pgn);
            StringAssert.Contains("[Black \"Engine v0\"]", pgn);
            StringAssert.Contains("[Result \"*\"]", pgn);
        }

        [Test]
        public void FoolsMateMoveSequence_WritesExpectedMovetextAndResult()
        {
            var startBoard = FenParser.Parse(FenParserTests.StartFen);
            var moves = new List<Move>
            {
                MakeMoveByAlgebraic(startBoard, "f2", "f3"),
                MakeMoveByAlgebraic(ApplySequence(startBoard, "f2-f3"), "e7", "e5"),
                MakeMoveByAlgebraic(ApplySequence(startBoard, "f2-f3", "e7-e5"), "g2", "g4"),
                MakeMoveByAlgebraic(ApplySequence(startBoard, "f2-f3", "e7-e5", "g2-g4"), "d8", "h4")
            };
            var pgn = PgnWriter.Write(startBoard, moves, GameResult.BlackWinsByCheckmate);
            StringAssert.Contains("1. f3 e5 2. g4 Qh4# 0-1", pgn);
            StringAssert.Contains("[Result \"0-1\"]", pgn);
        }

        [Test]
        public void GameEndingOnWhiteMove_OmitsBlackPlyOnFinalMoveNumber()
        {
            var startBoard = FenParser.Parse("7k/8/6K1/8/8/5Q2/8/8 w - - 0 1");
            var queenF3ToF8Mate = MakeMoveByAlgebraic(startBoard, "f3", "f8");
            var pgn = PgnWriter.Write(startBoard, new[] { queenF3ToF8Mate }, GameResult.WhiteWinsByCheckmate);
            StringAssert.Contains("1. Qf8# 1-0", pgn);
        }

        [Test]
        public void DrawResult_IsHalfDashHalf()
        {
            var startBoard = FenParser.Parse(FenParserTests.StartFen);
            var pgn = PgnWriter.Write(startBoard, new List<Move>(), GameResult.DrawByStalemate);
            StringAssert.Contains("[Result \"1/2-1/2\"]", pgn);
            StringAssert.Contains("1/2-1/2", pgn);
        }

        private static Move MakeMoveByAlgebraic(BoardState board, string fromAlgebraic, string toAlgebraic)
        {
            int fromSquareIndex = Square.FromAlgebraic(fromAlgebraic);
            int toSquareIndex = Square.FromAlgebraic(toAlgebraic);
            var legalMoves = new List<Move>(64);
            MoveGenerator.GenerateLegal(board, legalMoves);
            foreach (var legalMove in legalMoves)
                if (legalMove.FromSquare == fromSquareIndex && legalMove.ToSquare == toSquareIndex)
                    return legalMove;
            throw new System.ArgumentException($"No legal move {fromAlgebraic}-{toAlgebraic}");
        }

        private static BoardState ApplySequence(BoardState startBoard, params string[] moveSpecs)
        {
            var board = startBoard.Clone();
            foreach (var moveSpec in moveSpecs)
            {
                var parts = moveSpec.Split('-');
                var move = MakeMoveByAlgebraic(board, parts[0], parts[1]);
                MoveExecutor.MakeMove(board, move);
            }
            return board;
        }
    }
}
