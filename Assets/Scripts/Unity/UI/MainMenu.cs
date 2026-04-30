using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Chess.Core.Board;
using Chess.Core.Search;

namespace Chess.Unity.UI
{
    public sealed class MainMenu : MonoBehaviour
    {
        public Button EasyDifficultyButton;
        public Button MediumDifficultyButton;
        public Button HardDifficultyButton;
        public Button PlayAsWhiteButton;
        public Button PlayAsBlackButton;
        public Button QuitButton;
        public Color SelectedDifficultyColor = new Color(0.30f, 0.55f, 0.85f, 1f);
        public Color UnselectedDifficultyColor = new Color(0.18f, 0.20f, 0.26f, 1f);
        public string GameSceneName = "Game";

        private Difficulty _currentlySelectedDifficulty;

        private void Awake()
        {
            _currentlySelectedDifficulty = GamePreferences.LoadDifficulty();
            EasyDifficultyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Easy));
            MediumDifficultyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Medium));
            HardDifficultyButton.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
            PlayAsWhiteButton.onClick.AddListener(() => StartNewGame(PieceColor.White));
            PlayAsBlackButton.onClick.AddListener(() => StartNewGame(PieceColor.Black));
            QuitButton.onClick.AddListener(QuitApplication);
            RefreshDifficultyButtonHighlights();
        }

        private void SelectDifficulty(Difficulty difficulty)
        {
            _currentlySelectedDifficulty = difficulty;
            GamePreferences.SaveDifficulty(difficulty);
            RefreshDifficultyButtonHighlights();
        }

        private void RefreshDifficultyButtonHighlights()
        {
            ApplyHighlightToButton(EasyDifficultyButton, _currentlySelectedDifficulty == Difficulty.Easy);
            ApplyHighlightToButton(MediumDifficultyButton, _currentlySelectedDifficulty == Difficulty.Medium);
            ApplyHighlightToButton(HardDifficultyButton, _currentlySelectedDifficulty == Difficulty.Hard);
        }

        private void ApplyHighlightToButton(Button difficultyButton, bool isSelected)
        {
            if (difficultyButton == null) return;
            var backgroundImage = difficultyButton.GetComponent<Image>();
            if (backgroundImage != null) backgroundImage.color = isSelected ? SelectedDifficultyColor : UnselectedDifficultyColor;
        }

        private void StartNewGame(PieceColor humanColor)
        {
            GamePreferences.SaveHumanColor(humanColor);
            GamePreferences.SaveDifficulty(_currentlySelectedDifficulty);
            SceneManager.LoadScene(GameSceneName);
        }

        private void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
