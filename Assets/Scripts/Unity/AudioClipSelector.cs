using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Rules;

namespace Chess.Unity
{
    public enum AudioClipKind
    {
        Move,
        Capture,
        Check,
        Select,
        Checkmate,
        Victory,
        Defeat,
        Draw
    }

    public static class AudioClipSelector
    {
        public static AudioClipKind SelectMoveKind(Move appliedMove, BoardState postMoveBoard, GameResult resultAfterMove)
        {
            if (IsCheckmate(resultAfterMove)) return AudioClipKind.Checkmate;
            if (GameStateChecker.InCheck(postMoveBoard)) return AudioClipKind.Check;
            if (appliedMove.IsCapture) return AudioClipKind.Capture;
            return AudioClipKind.Move;
        }

        public static AudioClipKind SelectOutcomeKind(GameResult result, PieceColor humanColor)
        {
            if (!IsCheckmate(result)) return AudioClipKind.Draw;
            bool humanWon = (result == GameResult.WhiteWinsByCheckmate && humanColor == PieceColor.White)
                         || (result == GameResult.BlackWinsByCheckmate && humanColor == PieceColor.Black);
            return humanWon ? AudioClipKind.Victory : AudioClipKind.Defeat;
        }

        private static bool IsCheckmate(GameResult result)
        {
            return result == GameResult.WhiteWinsByCheckmate || result == GameResult.BlackWinsByCheckmate;
        }
    }
}
