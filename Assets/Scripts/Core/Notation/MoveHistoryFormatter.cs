using System.Collections.Generic;
using System.Text;
using Chess.Core.Board;

namespace Chess.Core.Notation
{
    public static class MoveHistoryFormatter
    {
        public static string Format(IReadOnlyList<string> sanByPly, PieceColor startingSide, int startingFullmoveNumber)
        {
            if (sanByPly == null || sanByPly.Count == 0) return string.Empty;

            var historyBuilder = new StringBuilder();
            int currentFullmoveNumber = startingFullmoveNumber;
            int plyIndex = 0;

            if (startingSide == PieceColor.Black)
            {
                historyBuilder.Append(currentFullmoveNumber).Append("... ").Append(sanByPly[0]);
                plyIndex = 1;
                currentFullmoveNumber++;
            }

            while (plyIndex < sanByPly.Count)
            {
                if (historyBuilder.Length > 0) historyBuilder.Append('\n');
                historyBuilder.Append(currentFullmoveNumber).Append(". ").Append(sanByPly[plyIndex]);
                plyIndex++;
                if (plyIndex < sanByPly.Count)
                {
                    historyBuilder.Append(' ').Append(sanByPly[plyIndex]);
                    plyIndex++;
                }
                currentFullmoveNumber++;
            }

            return historyBuilder.ToString();
        }
    }
}
