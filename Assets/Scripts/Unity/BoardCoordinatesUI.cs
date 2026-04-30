using UnityEngine;
using TMPro;

namespace Chess.Unity
{
    [ExecuteAlways]
    public sealed class BoardCoordinatesUI : MonoBehaviour
    {
        public BoardView Board;
        public GameObject LabelPrefab;
        public Transform Container;
        public bool ShowCoordinates = true;

        private bool _hasSpawnedLabels;

        private void Start()
        {
            if (Application.isPlaying) SpawnLabelsIfNeeded();
            ApplyVisibility();
        }

        private void OnValidate() => ApplyVisibility();

        private void SpawnLabelsIfNeeded()
        {
            if (_hasSpawnedLabels || Container == null || LabelPrefab == null) return;
            for (int fileIndex = 0; fileIndex < 8; fileIndex++)
            {
                string fileLabel = ((char)('a' + fileIndex)).ToString();
                Spawn(fileLabel, new Vector3(fileIndex - 3.5f, -4.2f, 0f));
                Spawn(fileLabel, new Vector3(fileIndex - 3.5f, 4.2f, 0f));
            }
            for (int rankIndex = 0; rankIndex < 8; rankIndex++)
            {
                string rankLabel = (rankIndex + 1).ToString();
                Spawn(rankLabel, new Vector3(-4.2f, rankIndex - 3.5f, 0f));
                Spawn(rankLabel, new Vector3(4.2f, rankIndex - 3.5f, 0f));
            }
            _hasSpawnedLabels = true;
        }

        private void Spawn(string labelText, Vector3 localPosition)
        {
            var labelGameObject = Instantiate(LabelPrefab, Container);
            labelGameObject.transform.localPosition = localPosition;
            labelGameObject.GetComponent<TMP_Text>().text = labelText;
        }

        private void ApplyVisibility()
        {
            if (Container != null) Container.gameObject.SetActive(ShowCoordinates);
        }
    }
}
