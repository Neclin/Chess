using UnityEngine;
using Chess.Core.Search;

namespace Chess.Unity
{
    public static class OpeningBookBootstrap
    {
        private const string BookResourceName = "openings";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void LoadOpeningBookFromResources()
        {
            if (!Application.isPlaying) return;
            if (OpeningBook.Default != null) return;

            TextAsset bookTextAsset = Resources.Load<TextAsset>(BookResourceName);
            if (bookTextAsset == null)
            {
                Debug.LogWarning($"Opening book resource '{BookResourceName}' not found. AI will run without a book.");
                return;
            }

            OpeningBook.Default = OpeningBook.LoadFromBytes(bookTextAsset.bytes);
            Debug.Log($"Loaded opening book with {OpeningBook.Default.PositionCount} positions.");
        }
    }
}
