using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Moves;

namespace Chess.Unity
{
    public sealed class InputController : MonoBehaviour
    {
        public BoardView Board;
        public System.Func<int, IReadOnlyList<Move>> GetLegalMovesFromSquare;
        public System.Action<Move> OnMoveChosen;
        public System.Action<int, IReadOnlyList<Move>> OnSelectionChanged;
        public System.Action OnSelectionCleared;

        private int _selectedSquareIndex = -1;

        private void Awake() => Board.OnSquareClicked = HandleSquareClicked;

        public void HandleSquareClicked(int clickedSquareIndex)
        {
            if (_selectedSquareIndex == -1)
            {
                var legalMovesFromClickedSquare = GetLegalMovesFromSquare(clickedSquareIndex);
                if (legalMovesFromClickedSquare.Count == 0) return;
                _selectedSquareIndex = clickedSquareIndex;
                OnSelectionChanged?.Invoke(clickedSquareIndex, legalMovesFromClickedSquare);
                return;
            }

            if (clickedSquareIndex == _selectedSquareIndex)
            {
                ClearSelection();
                return;
            }

            var legalMovesFromSelected = GetLegalMovesFromSquare(_selectedSquareIndex);
            foreach (var legalMove in legalMovesFromSelected)
            {
                if (legalMove.ToSquare == clickedSquareIndex)
                {
                    OnMoveChosen?.Invoke(legalMove);
                    ClearSelection();
                    return;
                }
            }

            var legalMovesFromNewSquare = GetLegalMovesFromSquare(clickedSquareIndex);
            if (legalMovesFromNewSquare.Count > 0)
            {
                _selectedSquareIndex = clickedSquareIndex;
                OnSelectionChanged?.Invoke(clickedSquareIndex, legalMovesFromNewSquare);
                return;
            }

            ClearSelection();
        }

        private void ClearSelection()
        {
            _selectedSquareIndex = -1;
            OnSelectionCleared?.Invoke();
        }
    }
}
