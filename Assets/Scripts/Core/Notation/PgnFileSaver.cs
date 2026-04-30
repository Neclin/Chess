using System;
using System.IO;
using System.Text;

namespace Chess.Core.Notation
{
    public static class PgnFileSaver
    {
        public const string DefaultSavegamesFolderName = "savegames";

        public static string DefaultDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), DefaultSavegamesFolderName);
        }

        public static string BuildFileName(DateTime timestamp, string whitePlayerName, string blackPlayerName)
        {
            string formattedTimestamp = timestamp.ToString("yyyyMMdd-HHmmss");
            string safeWhiteName = SanitiseForFilename(whitePlayerName);
            string safeBlackName = SanitiseForFilename(blackPlayerName);
            return $"{formattedTimestamp}_{safeWhiteName}_vs_{safeBlackName}.pgn";
        }

        public static string Save(string pgnContent, string targetDirectory, string fileName)
        {
            Directory.CreateDirectory(targetDirectory);
            string fullPath = Path.Combine(targetDirectory, fileName);
            File.WriteAllText(fullPath, pgnContent);
            return fullPath;
        }

        private static string SanitiseForFilename(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "Unknown";
            var builder = new StringBuilder(rawName.Length);
            foreach (char character in rawName)
            {
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_') builder.Append(character);
                else builder.Append('_');
            }
            return builder.ToString();
        }
    }
}
