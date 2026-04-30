using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Unity
{
    public static class GamePreferences
    {
        public const string HumanColorKey = "Chess.HumanColor";
        public const string DifficultyKey = "Chess.Difficulty";

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
    }
}
