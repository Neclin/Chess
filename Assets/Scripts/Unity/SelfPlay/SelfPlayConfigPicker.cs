using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Chess.Tools;

namespace Chess.Unity.UI
{
    public sealed class SelfPlayConfigPicker : MonoBehaviour
    {
        public SelfPlaySceneController Controller;
        public TMP_Dropdown ConfigADropdown;
        public TMP_Dropdown ConfigBDropdown;
        public TMP_InputField GamesCountField;
        public TMP_InputField DepthField;
        public TMP_InputField TimeMsField;
        public Button StartButton;

        private List<string> _configurationNamesInDropdownOrder;

        private void Start()
        {
            PopulateDropdowns();
            if (GamesCountField != null && string.IsNullOrEmpty(GamesCountField.text)) GamesCountField.text = "5";
            if (DepthField != null && string.IsNullOrEmpty(DepthField.text)) DepthField.text = "4";
            if (TimeMsField != null && string.IsNullOrEmpty(TimeMsField.text)) TimeMsField.text = "0";
            if (StartButton != null) StartButton.onClick.AddListener(HandleStartButtonClicked);
        }

        private void OnDestroy()
        {
            if (StartButton != null) StartButton.onClick.RemoveListener(HandleStartButtonClicked);
        }

        private void PopulateDropdowns()
        {
            _configurationNamesInDropdownOrder = new List<string>(SelfPlayArena.NamedConfigs.Keys);
            var dropdownOptions = new List<TMP_Dropdown.OptionData>();
            foreach (string configurationName in _configurationNamesInDropdownOrder)
                dropdownOptions.Add(new TMP_Dropdown.OptionData(configurationName));

            if (ConfigADropdown != null)
            {
                ConfigADropdown.ClearOptions();
                ConfigADropdown.AddOptions(dropdownOptions);
                ConfigADropdown.value = _configurationNamesInDropdownOrder.IndexOf("baseline");
                if (ConfigADropdown.value < 0) ConfigADropdown.value = 0;
                ConfigADropdown.RefreshShownValue();
            }
            if (ConfigBDropdown != null)
            {
                ConfigBDropdown.ClearOptions();
                ConfigBDropdown.AddOptions(dropdownOptions);
                ConfigBDropdown.value = _configurationNamesInDropdownOrder.IndexOf("full");
                if (ConfigBDropdown.value < 0) ConfigBDropdown.value = dropdownOptions.Count - 1;
                ConfigBDropdown.RefreshShownValue();
            }
        }

        private void HandleStartButtonClicked()
        {
            if (Controller == null) return;
            if (_configurationNamesInDropdownOrder == null || _configurationNamesInDropdownOrder.Count == 0)
            {
                Debug.LogError("[SelfPlayConfigPicker] No configs to choose from.");
                return;
            }

            int gamesCount = ParseIntFieldOrDefault(GamesCountField, defaultValue: 5);
            int searchDepth = ParseIntFieldOrDefault(DepthField, defaultValue: 4);
            int timeBudgetMs = ParseIntFieldOrDefault(TimeMsField, defaultValue: 0);
            if (timeBudgetMs > 0) searchDepth = 64;

            string selectedConfigA = _configurationNamesInDropdownOrder[Mathf.Clamp(ConfigADropdown != null ? ConfigADropdown.value : 0, 0, _configurationNamesInDropdownOrder.Count - 1)];
            string selectedConfigB = _configurationNamesInDropdownOrder[Mathf.Clamp(ConfigBDropdown != null ? ConfigBDropdown.value : 0, 0, _configurationNamesInDropdownOrder.Count - 1)];

            EngineConfig configurationA = SelfPlayArena.CloneNamedConfig(selectedConfigA, searchDepth, timeBudgetMs);
            EngineConfig configurationB = SelfPlayArena.CloneNamedConfig(selectedConfigB, searchDepth, timeBudgetMs);

            Controller.BeginSession(configurationA, configurationB, gamesCount);
        }

        private static int ParseIntFieldOrDefault(TMP_InputField inputField, int defaultValue)
        {
            if (inputField == null) return defaultValue;
            if (int.TryParse(inputField.text, out int parsedValue)) return parsedValue;
            return defaultValue;
        }
    }
}
