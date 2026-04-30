namespace Chess.Core.Rules
{
    public static class GameOverMessages
    {
        public static string Describe(GameResult result)
        {
            switch (result)
            {
                case GameResult.WhiteWinsByCheckmate:
                    return "Checkmate — White wins";
                case GameResult.BlackWinsByCheckmate:
                    return "Checkmate — Black wins";
                case GameResult.DrawByStalemate:
                    return "Stalemate — Draw";
                case GameResult.DrawByInsufficientMaterial:
                    return "Draw by insufficient material";
                case GameResult.DrawByFiftyMoveRule:
                    return "Draw by fifty-move rule";
                case GameResult.DrawByThreefoldRepetition:
                    return "Draw by threefold repetition";
                default:
                    return string.Empty;
            }
        }
    }
}
