using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Chess.Unity.UI
{
    public sealed class SelfPlaySummaryDialog : MonoBehaviour
    {
        public SelfPlaySceneController Controller;
        public TMP_Text SummaryLabel;
        public TMP_Text PgnPathLabel;
        public Button RunAgainButton;
        public Button ResetButton;

        private void Start()
        {
            if (RunAgainButton != null) RunAgainButton.onClick.AddListener(HandleRunAgainButtonClicked);
            if (ResetButton != null) ResetButton.onClick.AddListener(HandleResetButtonClicked);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (RunAgainButton != null) RunAgainButton.onClick.RemoveListener(HandleRunAgainButtonClicked);
            if (ResetButton != null) ResetButton.onClick.RemoveListener(HandleResetButtonClicked);
        }

        public void Show(string aName, int aWins, int aLosses,
                          string bName, int bWins, int bLosses,
                          int draws,
                          string pgnFolderPath)
        {
            if (SummaryLabel != null)
                SummaryLabel.text = $"{aName}: {aWins}W / {aLosses}L / {draws}D\n{bName}: {bWins}W / {bLosses}L / {draws}D";
            if (PgnPathLabel != null)
                PgnPathLabel.text = $"PGNs: {pgnFolderPath}";
            gameObject.SetActive(true);
        }

        private void HandleRunAgainButtonClicked()
        {
            if (Controller != null) Controller.RestartSession();
        }

        private void HandleResetButtonClicked()
        {
            if (Controller != null) Controller.ReturnToPicker();
        }
    }
}
