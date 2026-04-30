using NUnit.Framework;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class DifficultyPresetTests
    {
        [Test]
        public void Easy_uses_fixed_depth_two()
        {
            var settings = DifficultyPresets.SearchSettingsFor(Difficulty.Easy);
            Assert.AreEqual(2, settings.FixedDepth);
        }

        [Test]
        public void Medium_uses_fixed_depth_four()
        {
            var settings = DifficultyPresets.SearchSettingsFor(Difficulty.Medium);
            Assert.AreEqual(4, settings.FixedDepth);
        }

        [Test]
        public void Hard_uses_fixed_depth_six()
        {
            var settings = DifficultyPresets.SearchSettingsFor(Difficulty.Hard);
            Assert.AreEqual(6, settings.FixedDepth);
        }

        [Test]
        public void Default_difficulty_is_medium()
        {
            Assert.AreEqual(Difficulty.Medium, DifficultyPresets.DefaultDifficulty);
        }
    }
}
