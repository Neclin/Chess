using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Rules;

namespace Chess.Unity.UI
{
    public sealed class CapturedPiecesPanel : MonoBehaviour
    {
        public GameController GameController;
        public RectTransform WhiteCapturesRow;
        public RectTransform BlackCapturesRow;
        public TMP_Text MaterialBalanceLabel;

        [Tooltip("12 sprites indexed by (color * 6 + (pieceType - 1)) — same convention as BoardView.")]
        public Sprite[] PieceSprites = new Sprite[12];
        public float IconSize = 28f;

        private readonly List<CapturedPiece> _capturedPieces = new List<CapturedPiece>();

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
            if (undoInfo.CapturedPiece == PieceType.None) return;
            PieceColor capturerColor = undoInfo.CapturedColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            _capturedPieces.Add(new CapturedPiece(undoInfo.CapturedPiece, capturerColor));
            RebuildRows();
            UpdateBalanceLabel();
        }

        private void HandleGameReset()
        {
            _capturedPieces.Clear();
            ClearChildren(WhiteCapturesRow);
            ClearChildren(BlackCapturesRow);
            UpdateBalanceLabel();
        }

        public void ResetForExternalDriver()
        {
            _capturedPieces.Clear();
            ClearChildren(WhiteCapturesRow);
            ClearChildren(BlackCapturesRow);
            UpdateBalanceLabel();
        }

        public void RecordCapture(PieceType capturedPieceType, PieceColor capturerColor)
        {
            _capturedPieces.Add(new CapturedPiece(capturedPieceType, capturerColor));
            RebuildRows();
            UpdateBalanceLabel();
        }

        private void RebuildRows()
        {
            ClearChildren(WhiteCapturesRow);
            ClearChildren(BlackCapturesRow);

            var piecesCapturedByWhite = new List<PieceType>();
            var piecesCapturedByBlack = new List<PieceType>();
            for (int captureIndex = 0; captureIndex < _capturedPieces.Count; captureIndex++)
            {
                var capturedPiece = _capturedPieces[captureIndex];
                if (capturedPiece.CapturerColor == PieceColor.White) piecesCapturedByWhite.Add(capturedPiece.PieceType);
                else piecesCapturedByBlack.Add(capturedPiece.PieceType);
            }
            piecesCapturedByWhite.Sort(ComparePieceTypeByMaterialOrder);
            piecesCapturedByBlack.Sort(ComparePieceTypeByMaterialOrder);

            foreach (var pieceTypeCapturedByWhite in piecesCapturedByWhite)
                SpawnIcon(pieceTypeCapturedByWhite, PieceColor.Black, WhiteCapturesRow);
            foreach (var pieceTypeCapturedByBlack in piecesCapturedByBlack)
                SpawnIcon(pieceTypeCapturedByBlack, PieceColor.White, BlackCapturesRow);
        }

        private static int ComparePieceTypeByMaterialOrder(PieceType leftPiece, PieceType rightPiece)
            => ((int)leftPiece).CompareTo((int)rightPiece);

        private void SpawnIcon(PieceType capturedPieceType, PieceColor capturedPieceColor, RectTransform parentRow)
        {
            if (parentRow == null) return;
            int spriteSlotIndex = BoardView.SpriteIndex(capturedPieceType, capturedPieceColor);
            Sprite capturedPieceSprite = (PieceSprites != null && spriteSlotIndex >= 0 && spriteSlotIndex < PieceSprites.Length)
                ? PieceSprites[spriteSlotIndex]
                : null;
            var iconGameObject = new GameObject($"Captured_{capturedPieceColor}_{capturedPieceType}", typeof(RectTransform), typeof(Image));
            iconGameObject.transform.SetParent(parentRow, false);
            var iconRect = (RectTransform)iconGameObject.transform;
            iconRect.sizeDelta = new Vector2(IconSize, IconSize);
            var iconImage = iconGameObject.GetComponent<Image>();
            iconImage.sprite = capturedPieceSprite;
            iconImage.preserveAspect = true;
            var iconLayout = iconGameObject.AddComponent<LayoutElement>();
            iconLayout.minWidth = IconSize;
            iconLayout.minHeight = IconSize;
            iconLayout.preferredWidth = IconSize;
            iconLayout.preferredHeight = IconSize;
        }

        private void UpdateBalanceLabel()
        {
            if (MaterialBalanceLabel == null) return;
            int signedScore = MaterialBalance.WhiteMinusBlackScore(_capturedPieces);
            if (signedScore == 0) MaterialBalanceLabel.text = string.Empty;
            else if (signedScore > 0) MaterialBalanceLabel.text = $"White +{signedScore}";
            else MaterialBalanceLabel.text = $"Black +{-signedScore}";
        }

        private static void ClearChildren(Transform parentTransform)
        {
            if (parentTransform == null) return;
            for (int childIndex = parentTransform.childCount - 1; childIndex >= 0; childIndex--)
                Destroy(parentTransform.GetChild(childIndex).gameObject);
        }
    }
}
