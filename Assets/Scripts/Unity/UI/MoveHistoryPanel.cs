using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Notation;

namespace Chess.Unity.UI
{
    public sealed class MoveHistoryPanel : MonoBehaviour
    {
        public GameController GameController;
        public TMP_Text HistoryText;

        private readonly List<string> _sanByPly = new List<string>();
        private PieceColor _startingSide = PieceColor.White;
        private int _startingFullmoveNumber = 1;

        private void Awake()
        {
            if (HistoryText != null) HistoryText.text = string.Empty;
        }

        private void OnEnable()
        {
            if (GameController == null) return;
            GameController.OnMoveApplied += HandleMoveApplied;
            GameController.OnGameReset += HandleGameReset;
        }

        private void OnDisable()
        {
            if (GameController == null) return;
            GameController.OnMoveApplied -= HandleMoveApplied;
            GameController.OnGameReset -= HandleGameReset;
        }

        private void HandleMoveApplied(Move appliedMove, string sanText, UndoInfo undoInfo)
        {
            if (_sanByPly.Count == 0 && GameController.InitialBoardState != null)
            {
                _startingSide = GameController.InitialBoardState.SideToMove;
                _startingFullmoveNumber = GameController.InitialBoardState.FullmoveNumber;
            }
            _sanByPly.Add(sanText);
            Render();
        }

        private void HandleGameReset()
        {
            _sanByPly.Clear();
            if (HistoryText != null) HistoryText.text = string.Empty;
        }

        public void ResetForExternalDriver(PieceColor startingSide, int startingFullmoveNumber)
        {
            _sanByPly.Clear();
            _startingSide = startingSide;
            _startingFullmoveNumber = startingFullmoveNumber;
            if (HistoryText != null) HistoryText.text = string.Empty;
        }

        public void AppendSanMove(string sanText)
        {
            _sanByPly.Add(sanText);
            Render();
        }

        private void Render()
        {
            if (HistoryText == null) return;
            HistoryText.text = MoveHistoryFormatter.Format(_sanByPly, _startingSide, _startingFullmoveNumber);
        }
    }
}
