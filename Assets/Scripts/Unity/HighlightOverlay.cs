using System.Collections.Generic;
using UnityEngine;

namespace Chess.Unity
{
    public sealed class HighlightOverlay : MonoBehaviour
    {
        public BoardView Board;

        private readonly HashSet<int> _activelyHighlightedSquares = new HashSet<int>();

        public void Clear()
        {
            foreach (var squareIndex in _activelyHighlightedSquares)
                Board.GetSquareView(squareIndex).SetHighlightState(SquareHighlightState.Normal);
            _activelyHighlightedSquares.Clear();
        }

        public void ShowSelection(int squareIndex)
        {
            Board.GetSquareView(squareIndex).SetHighlightState(SquareHighlightState.Selected);
            _activelyHighlightedSquares.Add(squareIndex);
        }

        public void ShowMoveTarget(int squareIndex, bool isCapture)
        {
            Board.GetSquareView(squareIndex).SetHighlightState(
                isCapture ? SquareHighlightState.CaptureTarget : SquareHighlightState.MoveTarget);
            _activelyHighlightedSquares.Add(squareIndex);
        }

        public void ShowLastMove(int fromSquareIndex, int toSquareIndex)
        {
            Board.GetSquareView(fromSquareIndex).SetHighlightState(SquareHighlightState.LastMove);
            Board.GetSquareView(toSquareIndex).SetHighlightState(SquareHighlightState.LastMove);
            _activelyHighlightedSquares.Add(fromSquareIndex);
            _activelyHighlightedSquares.Add(toSquareIndex);
        }
    }
}
