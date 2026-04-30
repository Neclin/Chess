using System;
using System.Collections.Generic;
using System.Text;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Rules;

namespace Chess.Core.Notation
{
    public sealed class PgnHeaders
    {
        public string Event = "Casual";
        public string Site = "Local";
        public string Date = "";
        public string Round = "1";
        public string White = "Player";
        public string Black = "Engine v0";

        public static PgnHeaders WithDateToday() =>
            new PgnHeaders { Date = DateTime.Today.ToString("yyyy.MM.dd") };
    }

    public static class PgnWriter
    {
        public static string Write(
            BoardState startPosition,
            IEnumerable<Move> moves,
            GameResult result,
            PgnHeaders headers = null)
        {
            var resolvedHeaders = headers ?? PgnHeaders.WithDateToday();
            string resultTag = ResultTag(result);
            var pgnBuilder = new StringBuilder();
            AppendHeaderLine(pgnBuilder, "Event", resolvedHeaders.Event);
            AppendHeaderLine(pgnBuilder, "Site", resolvedHeaders.Site);
            AppendHeaderLine(pgnBuilder, "Date", resolvedHeaders.Date);
            AppendHeaderLine(pgnBuilder, "Round", resolvedHeaders.Round);
            AppendHeaderLine(pgnBuilder, "White", resolvedHeaders.White);
            AppendHeaderLine(pgnBuilder, "Black", resolvedHeaders.Black);
            AppendHeaderLine(pgnBuilder, "Result", resultTag);
            pgnBuilder.Append('\n');

            var workingBoard = startPosition.Clone();
            bool firstToken = true;
            foreach (var move in moves)
            {
                if (workingBoard.SideToMove == PieceColor.White)
                {
                    if (!firstToken) pgnBuilder.Append(' ');
                    pgnBuilder.Append(workingBoard.FullmoveNumber).Append('.').Append(' ');
                }
                else
                {
                    pgnBuilder.Append(' ');
                }
                pgnBuilder.Append(San.ToSan(workingBoard, move));
                MoveExecutor.MakeMove(workingBoard, move);
                firstToken = false;
            }

            if (!firstToken) pgnBuilder.Append(' ');
            pgnBuilder.Append(resultTag);
            return pgnBuilder.ToString();
        }

        private static void AppendHeaderLine(StringBuilder pgnBuilder, string tagName, string tagValue)
        {
            pgnBuilder.Append('[').Append(tagName).Append(' ').Append('"').Append(EscapeHeaderValue(tagValue)).Append('"').Append(']').Append('\n');
        }

        private static string EscapeHeaderValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.IndexOf('"') < 0 && value.IndexOf('\\') < 0) return value;
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string ResultTag(GameResult result) => result switch
        {
            GameResult.WhiteWinsByCheckmate => "1-0",
            GameResult.BlackWinsByCheckmate => "0-1",
            GameResult.DrawByStalemate => "1/2-1/2",
            GameResult.DrawByInsufficientMaterial => "1/2-1/2",
            GameResult.DrawByFiftyMoveRule => "1/2-1/2",
            GameResult.DrawByThreefoldRepetition => "1/2-1/2",
            _ => "*"
        };
    }
}
