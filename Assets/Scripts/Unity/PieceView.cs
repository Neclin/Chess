using UnityEngine;
using Chess.Core.Board;

namespace Chess.Unity
{
    public sealed class PieceView : MonoBehaviour
    {
        public PieceType PieceType;
        public PieceColor PieceColor;
        public int SquareIndex;

        private SpriteRenderer _spriteRenderer;

        private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();

        public void SetPiece(PieceType pieceType, PieceColor pieceColor, Sprite pieceSprite)
        {
            PieceType = pieceType;
            PieceColor = pieceColor;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = pieceSprite;
        }
    }
}
