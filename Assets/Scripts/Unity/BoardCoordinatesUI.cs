using UnityEngine;
using TMPro;

namespace Chess.Unity
{
    public sealed class BoardCoordinatesUI : MonoBehaviour
    {
        public BoardView Board;
        public GameObject LabelPrefab;
        public Transform Container;

        private void Start()
        {
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
        }

        private void Spawn(string labelText, Vector3 localPosition)
        {
            var labelGameObject = Instantiate(LabelPrefab, Container);
            labelGameObject.transform.localPosition = localPosition;
            labelGameObject.GetComponent<TMP_Text>().text = labelText;
        }
    }
}
