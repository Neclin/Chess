using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Chess.Core.Board;
using Chess.Core.Notation;
using Chess.Core.Rules;

namespace Chess.Unity.UI
{
    public sealed class GameOverDialog : MonoBehaviour
    {
        public GameObject Root;
        public TMP_Text MessageLabel;
        public TMP_Text SavePgnFeedbackLabel;
        public Button RematchButton;
        public Button SwitchSidesButton;
        public Button SavePgnButton;
        public Button MainMenuButton;
        public string MainMenuSceneName = "MainMenu";
        public string WhitePlayerName = "Player";
        public string BlackPlayerName = "Engine v0";

        private GameController _activeGameController;

        private void Awake()
        {
            if (Root != null) Root.SetActive(false);
            if (SavePgnFeedbackLabel != null) SavePgnFeedbackLabel.text = string.Empty;
            RematchButton.onClick.AddListener(OnRematchClicked);
            SwitchSidesButton.onClick.AddListener(OnSwitchSidesClicked);
            SavePgnButton.onClick.AddListener(OnSavePgnClicked);
            MainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void Show(GameController activeGameController)
        {
            _activeGameController = activeGameController;
            if (MessageLabel != null) MessageLabel.text = GameOverMessages.Describe(activeGameController.LastResult);
            if (SavePgnFeedbackLabel != null) SavePgnFeedbackLabel.text = string.Empty;
            if (Root != null) Root.SetActive(true);
        }

        public void Hide()
        {
            if (Root != null) Root.SetActive(false);
        }

        private void OnRematchClicked()
        {
            if (_activeGameController == null) return;
            PieceColor sameHumanColor = _activeGameController.HumanColor;
            Hide();
            _activeGameController.Restart(sameHumanColor);
        }

        private void OnSwitchSidesClicked()
        {
            if (_activeGameController == null) return;
            PieceColor switchedHumanColor = _activeGameController.HumanColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            SettingsStore.SaveHumanColor(switchedHumanColor);
            Hide();
            _activeGameController.Restart(switchedHumanColor);
        }

        private void OnSavePgnClicked()
        {
            if (_activeGameController == null) return;
            string whiteName = _activeGameController.HumanColor == PieceColor.White ? WhitePlayerName : BlackPlayerName;
            string blackName = _activeGameController.HumanColor == PieceColor.White ? BlackPlayerName : WhitePlayerName;
            var pgnHeaders = new PgnHeaders
            {
                Event = "Casual",
                Site = "Local",
                Date = DateTime.Today.ToString("yyyy.MM.dd"),
                Round = "1",
                White = whiteName,
                Black = blackName
            };
            string pgnContent = PgnWriter.Write(
                _activeGameController.InitialBoardState,
                _activeGameController.MovesPlayed,
                _activeGameController.LastResult,
                pgnHeaders);
            string targetDirectory = PgnFileSaver.DefaultDirectory();
            string fileName = PgnFileSaver.BuildFileName(DateTime.Now, whiteName, blackName);
            try
            {
                string writtenPath = PgnFileSaver.Save(pgnContent, targetDirectory, fileName);
                if (SavePgnFeedbackLabel != null) SavePgnFeedbackLabel.text = $"Saved to {writtenPath}";
                Debug.Log($"PGN saved to {writtenPath}");
            }
            catch (Exception saveException)
            {
                if (SavePgnFeedbackLabel != null) SavePgnFeedbackLabel.text = $"Save failed: {saveException.Message}";
                Debug.LogError($"PGN save failed: {saveException}");
            }
        }

        private void OnMainMenuClicked()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}
