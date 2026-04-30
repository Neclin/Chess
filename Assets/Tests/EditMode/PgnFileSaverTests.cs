using System;
using System.IO;
using NUnit.Framework;
using Chess.Core.Notation;

namespace Chess.Core.Tests
{
    public class PgnFileSaverTests
    {
        [Test]
        public void BuildFileName_uses_timestamp_player_names_and_pgn_extension()
        {
            string fileName = PgnFileSaver.BuildFileName(
                new DateTime(2026, 4, 30, 15, 30, 45, DateTimeKind.Local),
                "Player",
                "Engine v0");

            Assert.AreEqual("20260430-153045_Player_vs_Engine_v0.pgn", fileName);
        }

        [Test]
        public void BuildFileName_replaces_path_unsafe_characters_with_underscore()
        {
            string fileName = PgnFileSaver.BuildFileName(
                new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Local),
                "Lu/ke",
                "Stockfish:17");

            Assert.AreEqual("20260102-030405_Lu_ke_vs_Stockfish_17.pgn", fileName);
        }

        [Test]
        public void Save_writes_content_to_file_and_creates_directory()
        {
            string testDirectory = Path.Combine(Path.GetTempPath(), "ChessPgnFileSaverTest_" + Guid.NewGuid().ToString("N"));
            Assert.IsFalse(Directory.Exists(testDirectory));

            try
            {
                string writtenPath = PgnFileSaver.Save("game pgn body", testDirectory, "out.pgn");

                Assert.AreEqual(Path.Combine(testDirectory, "out.pgn"), writtenPath);
                Assert.IsTrue(File.Exists(writtenPath));
                Assert.AreEqual("game pgn body", File.ReadAllText(writtenPath));
            }
            finally
            {
                if (Directory.Exists(testDirectory)) Directory.Delete(testDirectory, recursive: true);
            }
        }

        [Test]
        public void DefaultDirectory_lives_under_project_working_directory()
        {
            string defaultDirectory = PgnFileSaver.DefaultDirectory();
            string projectWorkingDirectory = Directory.GetCurrentDirectory();

            Assert.IsTrue(defaultDirectory.StartsWith(projectWorkingDirectory));
            Assert.IsTrue(defaultDirectory.EndsWith(PgnFileSaver.DefaultSavegamesFolderName));
        }
    }
}
