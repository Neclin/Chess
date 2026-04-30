using System.Collections.Generic;
using UnityEngine;

namespace Chess.Unity
{
    public sealed class HighlightOverlay : MonoBehaviour
    {
        public BoardView Board;
        public GameObject DotPrefab;
        public GameObject CapturePrefab;
        public GameObject LastMovePrefab;
        public GameObject SelectionPrefab;

        private readonly List<GameObject> _spawnedHighlights = new List<GameObject>();

        public void Clear()
        {
            foreach (var highlightObject in _spawnedHighlights) Destroy(highlightObject);
            _spawnedHighlights.Clear();
        }

        public void ShowSelection(int squareIndex) => Spawn(SelectionPrefab, squareIndex);

        public void ShowMoveTarget(int squareIndex, bool isCapture) => Spawn(isCapture ? CapturePrefab : DotPrefab, squareIndex);

        public void ShowLastMove(int fromSquareIndex, int toSquareIndex)
        {
            Spawn(LastMovePrefab, fromSquareIndex);
            Spawn(LastMovePrefab, toSquareIndex);
        }

        private void Spawn(GameObject prefab, int squareIndex)
        {
            if (prefab == null) return;
            var spawnedObject = Instantiate(prefab, Board.transform);
            spawnedObject.transform.localPosition = Board.ToLocalPosition(squareIndex) + new Vector3(0, 0, -0.05f);
            _spawnedHighlights.Add(spawnedObject);
        }
    }
}
