using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Chess.Unity.UI
{
    public sealed class PauseMenu : MonoBehaviour
    {
        public GameObject Root;
        public Button ResumeButton;
        public Button SettingsButton;
        public Button MainMenuButton;
        public SettingsMenu SettingsMenu;
        public BoardView Board;
        public string MainMenuSceneName = "MainMenu";

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Root != null) Root.SetActive(false);
            if (ResumeButton != null) ResumeButton.onClick.AddListener(Resume);
            if (SettingsButton != null) SettingsButton.onClick.AddListener(OpenSettings);
            if (MainMenuButton != null) MainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (SettingsMenu != null)
            {
                SettingsMenu.OnClosed += OnSettingsMenuClosed;
                SettingsMenu.OnSettingsApplied += OnSettingsApplied;
            }
        }

        private void OnDestroy()
        {
            if (SettingsMenu != null)
            {
                SettingsMenu.OnClosed -= OnSettingsMenuClosed;
                SettingsMenu.OnSettingsApplied -= OnSettingsApplied;
            }
        }

        private void OnSettingsApplied()
        {
            if (Board != null) Board.ApplyTheme(SettingsStore.LoadBoardTheme());
        }

        private void Update()
        {
            Keyboard activeKeyboard = Keyboard.current;
            if (activeKeyboard == null) return;
            if (!activeKeyboard.escapeKey.wasPressedThisFrame) return;
            if (SettingsMenu != null && SettingsMenu.Root != null && SettingsMenu.Root.activeSelf)
            {
                SettingsMenu.Close();
                return;
            }
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused = true;
            if (Root != null) Root.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Resume()
        {
            IsPaused = false;
            if (Root != null) Root.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OpenSettings()
        {
            if (SettingsMenu == null) return;
            if (Root != null) Root.SetActive(false);
            SettingsMenu.Open();
        }

        private void OnSettingsMenuClosed()
        {
            if (IsPaused && Root != null) Root.SetActive(true);
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}
