using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;
using Chess.Core.Search;

namespace Chess.Tools
{
    public static class ReplayabilityCsvExporter
    {
        private const string OutputCsvPath = "docs/data/replayability.csv";
        private const string CsvHeader = "gameId,san,move1,move2,move3,move4,move5,move6";
        private const string StartPositionFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const int FixedSearchDepth = 4;
        private const int PliesToCapturePerGame = 6;
        private const int GamesToRun = 100;
        private const int RandomSeed = 0xC4E55;

        [MenuItem("Tools/Chess/Capture Replayability CSV")]
        public static void ExportReplayabilityCsvMenuItem()
        {
            EnsureOpeningBookLoaded();
            string csvText = BuildReplayabilityCsv();
            string outputDirectoryPath = Path.GetDirectoryName(OutputCsvPath);
            if (!string.IsNullOrEmpty(outputDirectoryPath) && !Directory.Exists(outputDirectoryPath))
                Directory.CreateDirectory(outputDirectoryPath);
            File.WriteAllText(OutputCsvPath, csvText);
            Debug.Log($"Wrote {GamesToRun} games to {OutputCsvPath}");
        }

        public static string BuildReplayabilityCsv()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine(CsvHeader);

            bool previousBookEnabled = OpeningBook.Enabled;
            System.Random previousSharedRandom = OpeningBook.SharedRandom;
            try
            {
                OpeningBook.SharedRandom = new System.Random(RandomSeed);
                OpeningBook.Enabled = true;
                for (int gameIndex = 0; gameIndex < GamesToRun; gameIndex++)
                    AppendGameRow(csvBuilder, gameId: gameIndex + 1);
            }
            finally
            {
                OpeningBook.Enabled = previousBookEnabled;
                OpeningBook.SharedRandom = previousSharedRandom;
            }

            return csvBuilder.ToString();
        }

        private static void AppendGameRow(StringBuilder csvBuilder, int gameId)
        {
            MinimaxSearch.ResetTTStats();
            BoardState board = FenParser.Parse(StartPositionFen);
            string[] capturedUciMoves = new string[PliesToCapturePerGame];
            string[] capturedSanMoves = new string[PliesToCapturePerGame];
            for (int plyIndex = 0; plyIndex < PliesToCapturePerGame; plyIndex++)
            {
                SearchResult searchResult = MinimaxSearch.FindBestMove(board, FixedSearchDepth);
                Move chosenMove = searchResult.BestMove;
                if (chosenMove.FromSquare == chosenMove.ToSquare) break;
                capturedUciMoves[plyIndex] = chosenMove.ToString();
                capturedSanMoves[plyIndex] = San.ToSan(board, chosenMove);
                MoveExecutor.MakeMove(board, chosenMove);
            }

            string sanSequenceString = FormatSanSequence(capturedSanMoves);
            csvBuilder.Append(gameId.ToString(CultureInfo.InvariantCulture))
                      .Append(',')
                      .Append('"').Append(sanSequenceString).Append('"');
            for (int plyIndex = 0; plyIndex < PliesToCapturePerGame; plyIndex++)
                csvBuilder.Append(',').Append(capturedUciMoves[plyIndex] ?? string.Empty);
            csvBuilder.AppendLine();
        }

        private static string FormatSanSequence(string[] capturedSanMoves)
        {
            var sanBuilder = new StringBuilder();
            for (int plyIndex = 0; plyIndex < capturedSanMoves.Length; plyIndex++)
            {
                if (string.IsNullOrEmpty(capturedSanMoves[plyIndex])) break;
                bool whiteToMove = (plyIndex % 2) == 0;
                if (whiteToMove)
                {
                    int fullmoveNumber = plyIndex / 2 + 1;
                    if (sanBuilder.Length > 0) sanBuilder.Append(' ');
                    sanBuilder.Append(fullmoveNumber).Append('.');
                }
                else
                {
                    sanBuilder.Append(' ');
                }
                sanBuilder.Append(capturedSanMoves[plyIndex]);
            }
            return sanBuilder.ToString();
        }

        private static void EnsureOpeningBookLoaded()
        {
            if (OpeningBook.Default != null) return;
            AssetDatabase.Refresh();
            TextAsset bookTextAsset = Resources.Load<TextAsset>("openings");
            if (bookTextAsset == null)
            {
                Debug.LogWarning("openings.bytes not found in Resources; rows will fall through to search.");
                return;
            }
            OpeningBook.Default = OpeningBook.LoadFromBytes(bookTextAsset.bytes);
        }
    }
}
