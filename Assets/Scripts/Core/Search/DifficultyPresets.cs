namespace Chess.Core.Search
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    public readonly struct SearchSettings
    {
        public readonly int FixedDepth;

        public SearchSettings(int fixedDepth)
        {
            FixedDepth = fixedDepth;
        }
    }

    public static class DifficultyPresets
    {
        public const Difficulty DefaultDifficulty = Difficulty.Medium;

        public static SearchSettings SearchSettingsFor(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return new SearchSettings(fixedDepth: 2);
                case Difficulty.Medium:
                    return new SearchSettings(fixedDepth: 4);
                case Difficulty.Hard:
                    return new SearchSettings(fixedDepth: 6);
                default:
                    return SearchSettingsFor(DefaultDifficulty);
            }
        }
    }
}
