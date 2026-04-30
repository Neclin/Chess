using NUnit.Framework;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Search;
using Chess.Unity;

namespace Chess.Core.Tests
{
    public class SettingsStoreTests
    {
        [SetUp]
        public void DeleteAllKeysBeforeEachTest()
        {
            PlayerPrefs.DeleteKey(SettingsStore.HumanColorKey);
            PlayerPrefs.DeleteKey(SettingsStore.DifficultyKey);
            PlayerPrefs.DeleteKey(SettingsStore.BoardThemeKey);
            PlayerPrefs.DeleteKey(SettingsStore.SoundEnabledKey);
            PlayerPrefs.DeleteKey(SettingsStore.ShowLegalMoveDotsKey);
            PlayerPrefs.DeleteKey(SettingsStore.FlipBoardAfterMoveKey);
        }

        [TearDown]
        public void DeleteAllKeysAfterEachTest()
        {
            DeleteAllKeysBeforeEachTest();
        }

        [Test]
        public void LoadHumanColor_WhenUnset_ReturnsNull()
        {
            Assert.IsNull(SettingsStore.LoadHumanColor());
        }

        [Test]
        public void SaveAndLoadHumanColor_RoundTrips()
        {
            SettingsStore.SaveHumanColor(PieceColor.Black);
            Assert.AreEqual(PieceColor.Black, SettingsStore.LoadHumanColor());

            SettingsStore.SaveHumanColor(PieceColor.White);
            Assert.AreEqual(PieceColor.White, SettingsStore.LoadHumanColor());
        }

        [Test]
        public void LoadDifficulty_WhenUnset_ReturnsDefault()
        {
            Assert.AreEqual(DifficultyPresets.DefaultDifficulty, SettingsStore.LoadDifficulty());
        }

        [Test]
        public void SaveAndLoadDifficulty_RoundTrips()
        {
            SettingsStore.SaveDifficulty(Difficulty.Easy);
            Assert.AreEqual(Difficulty.Easy, SettingsStore.LoadDifficulty());

            SettingsStore.SaveDifficulty(Difficulty.Hard);
            Assert.AreEqual(Difficulty.Hard, SettingsStore.LoadDifficulty());
        }

        [Test]
        public void LoadDifficulty_WithCorruptedValue_ReturnsDefault()
        {
            PlayerPrefs.SetInt(SettingsStore.DifficultyKey, 999);
            Assert.AreEqual(DifficultyPresets.DefaultDifficulty, SettingsStore.LoadDifficulty());
        }

        [Test]
        public void LoadBoardTheme_WhenUnset_ReturnsDefault()
        {
            Assert.AreEqual(SettingsStore.DefaultBoardTheme, SettingsStore.LoadBoardTheme());
        }

        [Test]
        public void SaveAndLoadBoardTheme_RoundTripsAllValues()
        {
            SettingsStore.SaveBoardTheme(BoardTheme.Wood);
            Assert.AreEqual(BoardTheme.Wood, SettingsStore.LoadBoardTheme());

            SettingsStore.SaveBoardTheme(BoardTheme.Light);
            Assert.AreEqual(BoardTheme.Light, SettingsStore.LoadBoardTheme());

            SettingsStore.SaveBoardTheme(BoardTheme.Dark);
            Assert.AreEqual(BoardTheme.Dark, SettingsStore.LoadBoardTheme());
        }

        [Test]
        public void LoadBoardTheme_WithCorruptedValue_ReturnsDefault()
        {
            PlayerPrefs.SetInt(SettingsStore.BoardThemeKey, 999);
            Assert.AreEqual(SettingsStore.DefaultBoardTheme, SettingsStore.LoadBoardTheme());
        }

        [Test]
        public void LoadSoundEnabled_WhenUnset_ReturnsTrue()
        {
            Assert.IsTrue(SettingsStore.LoadSoundEnabled());
        }

        [Test]
        public void SaveAndLoadSoundEnabled_RoundTrips()
        {
            SettingsStore.SaveSoundEnabled(false);
            Assert.IsFalse(SettingsStore.LoadSoundEnabled());

            SettingsStore.SaveSoundEnabled(true);
            Assert.IsTrue(SettingsStore.LoadSoundEnabled());
        }

        [Test]
        public void LoadShowLegalMoveDots_WhenUnset_ReturnsTrue()
        {
            Assert.IsTrue(SettingsStore.LoadShowLegalMoveDots());
        }

        [Test]
        public void SaveAndLoadShowLegalMoveDots_RoundTrips()
        {
            SettingsStore.SaveShowLegalMoveDots(false);
            Assert.IsFalse(SettingsStore.LoadShowLegalMoveDots());

            SettingsStore.SaveShowLegalMoveDots(true);
            Assert.IsTrue(SettingsStore.LoadShowLegalMoveDots());
        }

        [Test]
        public void LoadFlipBoardAfterMove_WhenUnset_ReturnsFalse()
        {
            Assert.IsFalse(SettingsStore.LoadFlipBoardAfterMove());
        }

        [Test]
        public void SaveAndLoadFlipBoardAfterMove_RoundTrips()
        {
            SettingsStore.SaveFlipBoardAfterMove(true);
            Assert.IsTrue(SettingsStore.LoadFlipBoardAfterMove());

            SettingsStore.SaveFlipBoardAfterMove(false);
            Assert.IsFalse(SettingsStore.LoadFlipBoardAfterMove());
        }
    }
}
