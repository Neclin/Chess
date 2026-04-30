using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Unity
{
    public enum BoardTheme
    {
        Wood = 0,
        Light = 1,
        Dark = 2
    }

    public static class SettingsStore
    {
        public const string HumanColorKey = "Chess.HumanColor";
        public const string DifficultyKey = "Chess.Difficulty";
        public const string BoardThemeKey = "Chess.BoardTheme";
        public const string SoundEnabledKey = "Chess.SoundEnabled";
        public const string ShowLegalMoveDotsKey = "Chess.ShowLegalMoveDots";
        public const string FlipBoardAfterMoveKey = "Chess.FlipBoardAfterMove";

        public const BoardTheme DefaultBoardTheme = BoardTheme.Wood;
        public const bool DefaultSoundEnabled = true;
        public const bool DefaultShowLegalMoveDots = true;
        public const bool DefaultFlipBoardAfterMove = false;

        public static void SaveHumanColor(PieceColor humanColor)
        {
            PlayerPrefs.SetInt(HumanColorKey, (int)humanColor);
            PlayerPrefs.Save();
        }

        public static PieceColor? LoadHumanColor()
        {
            if (!PlayerPrefs.HasKey(HumanColorKey)) return null;
            int storedValue = PlayerPrefs.GetInt(HumanColorKey);
            if (storedValue == (int)PieceColor.White) return PieceColor.White;
            if (storedValue == (int)PieceColor.Black) return PieceColor.Black;
            return null;
        }

        public static void SaveDifficulty(Difficulty difficulty)
        {
            PlayerPrefs.SetInt(DifficultyKey, (int)difficulty);
            PlayerPrefs.Save();
        }

        public static Difficulty LoadDifficulty()
        {
            if (!PlayerPrefs.HasKey(DifficultyKey)) return DifficultyPresets.DefaultDifficulty;
            int storedValue = PlayerPrefs.GetInt(DifficultyKey);
            if (storedValue >= (int)Difficulty.Easy && storedValue <= (int)Difficulty.Hard) return (Difficulty)storedValue;
            return DifficultyPresets.DefaultDifficulty;
        }

        public static void SaveBoardTheme(BoardTheme boardTheme)
        {
            PlayerPrefs.SetInt(BoardThemeKey, (int)boardTheme);
            PlayerPrefs.Save();
        }

        public static BoardTheme LoadBoardTheme()
        {
            if (!PlayerPrefs.HasKey(BoardThemeKey)) return DefaultBoardTheme;
            int storedValue = PlayerPrefs.GetInt(BoardThemeKey);
            if (storedValue >= (int)BoardTheme.Wood && storedValue <= (int)BoardTheme.Dark) return (BoardTheme)storedValue;
            return DefaultBoardTheme;
        }

        public static void SaveSoundEnabled(bool soundEnabled)
        {
            PlayerPrefs.SetInt(SoundEnabledKey, soundEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadSoundEnabled()
        {
            if (!PlayerPrefs.HasKey(SoundEnabledKey)) return DefaultSoundEnabled;
            return PlayerPrefs.GetInt(SoundEnabledKey) != 0;
        }

        public static void SaveShowLegalMoveDots(bool showLegalMoveDots)
        {
            PlayerPrefs.SetInt(ShowLegalMoveDotsKey, showLegalMoveDots ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadShowLegalMoveDots()
        {
            if (!PlayerPrefs.HasKey(ShowLegalMoveDotsKey)) return DefaultShowLegalMoveDots;
            return PlayerPrefs.GetInt(ShowLegalMoveDotsKey) != 0;
        }

        public static void SaveFlipBoardAfterMove(bool flipBoardAfterMove)
        {
            PlayerPrefs.SetInt(FlipBoardAfterMoveKey, flipBoardAfterMove ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadFlipBoardAfterMove()
        {
            if (!PlayerPrefs.HasKey(FlipBoardAfterMoveKey)) return DefaultFlipBoardAfterMove;
            return PlayerPrefs.GetInt(FlipBoardAfterMoveKey) != 0;
        }
    }
}
