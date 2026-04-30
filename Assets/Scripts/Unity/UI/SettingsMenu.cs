using System;
using UnityEngine;
using UnityEngine.UI;

namespace Chess.Unity.UI
{
    public sealed class SettingsMenu : MonoBehaviour
    {
        public GameObject Root;
        public Button ThemeWoodButton;
        public Button ThemeLightButton;
        public Button ThemeDarkButton;
        public Toggle SoundToggle;
        public Toggle ShowLegalMoveDotsToggle;
        public Toggle FlipBoardAfterMoveToggle;
        public Button BackButton;
        public Color SelectedThemeButtonColor = new Color(0.30f, 0.55f, 0.85f, 1f);
        public Color UnselectedThemeButtonColor = new Color(0.18f, 0.20f, 0.26f, 1f);

        public event Action OnClosed;
        public event Action OnSettingsApplied;

        private void Awake()
        {
            if (Root != null) Root.SetActive(false);
            if (ThemeWoodButton != null) ThemeWoodButton.onClick.AddListener(() => SelectTheme(BoardTheme.Wood));
            if (ThemeLightButton != null) ThemeLightButton.onClick.AddListener(() => SelectTheme(BoardTheme.Light));
            if (ThemeDarkButton != null) ThemeDarkButton.onClick.AddListener(() => SelectTheme(BoardTheme.Dark));
            if (SoundToggle != null) SoundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
            if (ShowLegalMoveDotsToggle != null) ShowLegalMoveDotsToggle.onValueChanged.AddListener(OnShowLegalMoveDotsToggleChanged);
            if (FlipBoardAfterMoveToggle != null) FlipBoardAfterMoveToggle.onValueChanged.AddListener(OnFlipBoardAfterMoveToggleChanged);
            if (BackButton != null) BackButton.onClick.AddListener(Close);
        }

        public void Open()
        {
            SyncControlsFromStore();
            if (Root != null) Root.SetActive(true);
        }

        public void Close()
        {
            if (Root != null) Root.SetActive(false);
            OnClosed?.Invoke();
        }

        private void SyncControlsFromStore()
        {
            BoardTheme storedTheme = SettingsStore.LoadBoardTheme();
            RefreshThemeButtonHighlights(storedTheme);
            if (SoundToggle != null) SoundToggle.SetIsOnWithoutNotify(SettingsStore.LoadSoundEnabled());
            if (ShowLegalMoveDotsToggle != null) ShowLegalMoveDotsToggle.SetIsOnWithoutNotify(SettingsStore.LoadShowLegalMoveDots());
            if (FlipBoardAfterMoveToggle != null) FlipBoardAfterMoveToggle.SetIsOnWithoutNotify(SettingsStore.LoadFlipBoardAfterMove());
        }

        private void SelectTheme(BoardTheme chosenTheme)
        {
            SettingsStore.SaveBoardTheme(chosenTheme);
            RefreshThemeButtonHighlights(chosenTheme);
            OnSettingsApplied?.Invoke();
        }

        private void RefreshThemeButtonHighlights(BoardTheme selectedTheme)
        {
            ApplyThemeButtonHighlight(ThemeWoodButton, selectedTheme == BoardTheme.Wood);
            ApplyThemeButtonHighlight(ThemeLightButton, selectedTheme == BoardTheme.Light);
            ApplyThemeButtonHighlight(ThemeDarkButton, selectedTheme == BoardTheme.Dark);
        }

        private void ApplyThemeButtonHighlight(Button themeButton, bool isSelected)
        {
            if (themeButton == null) return;
            var backgroundImage = themeButton.GetComponent<Image>();
            if (backgroundImage != null) backgroundImage.color = isSelected ? SelectedThemeButtonColor : UnselectedThemeButtonColor;
        }

        private void OnSoundToggleChanged(bool isOn)
        {
            SettingsStore.SaveSoundEnabled(isOn);
            OnSettingsApplied?.Invoke();
        }

        private void OnShowLegalMoveDotsToggleChanged(bool isOn)
        {
            SettingsStore.SaveShowLegalMoveDots(isOn);
            OnSettingsApplied?.Invoke();
        }

        private void OnFlipBoardAfterMoveToggleChanged(bool isOn)
        {
            SettingsStore.SaveFlipBoardAfterMove(isOn);
            OnSettingsApplied?.Invoke();
        }
    }
}
