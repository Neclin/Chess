using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Unity
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject LightSquarePrefab;
        public GameObject DarkSquarePrefab;
        public GameObject PiecePrefab;

        [Header("Piece sprites — 12 entries indexed by (color * 6 + (pieceType - 1))")]
        public Sprite[] PieceSprites = new Sprite[12];

        public System.Action<int> OnSquareClicked;
        public bool FlipForBlack;

        private readonly SquareView[] _squareViews = new SquareView[64];
        private readonly Dictionary<int, PieceView> _pieceViewsBySquare = new Dictionary<int, PieceView>();

        public void Build()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                int fileIndex = Square.FileIndex(squareIndex);
                int rankIndex = Square.RankIndex(squareIndex);
                bool isLightSquare = (fileIndex + rankIndex) % 2 == 1;
                var squareGameObject = Instantiate(isLightSquare ? LightSquarePrefab : DarkSquarePrefab, transform);
                squareGameObject.transform.localPosition = ToLocalPosition(squareIndex);
                squareGameObject.name = $"Sq_{Square.ToAlgebraic(squareIndex)}";
                var squareView = squareGameObject.GetComponent<SquareView>();
                squareView.SquareIndex = squareIndex;
                int capturedSquareIndex = squareIndex;
                squareView.OnClick = clickedSquareIndex => OnSquareClicked?.Invoke(clickedSquareIndex);
                _squareViews[squareIndex] = squareView;
            }
        }

        public void Sync(BoardState boardState)
        {
            foreach (var pieceView in _pieceViewsBySquare.Values) Destroy(pieceView.gameObject);
            _pieceViewsBySquare.Clear();

            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                PieceType pieceType = boardState.PieceAt(squareIndex, out PieceColor pieceColor);
                if (pieceType == PieceType.None) continue;
                var pieceGameObject = Instantiate(PiecePrefab, transform);
                pieceGameObject.transform.localPosition = ToLocalPosition(squareIndex) + new Vector3(0, 0, -0.1f);
                pieceGameObject.name = $"P_{pieceColor}_{pieceType}_{Square.ToAlgebraic(squareIndex)}";
                var pieceView = pieceGameObject.GetComponent<PieceView>();
                pieceView.SquareIndex = squareIndex;
                pieceView.SetPiece(pieceType, pieceColor, PieceSprites[SpriteIndex(pieceType, pieceColor)]);
                _pieceViewsBySquare[squareIndex] = pieceView;
            }
        }

        public Vector3 ToLocalPosition(int squareIndex)
        {
            int fileIndex = Square.FileIndex(squareIndex);
            int rankIndex = Square.RankIndex(squareIndex);
            if (FlipForBlack)
            {
                fileIndex = 7 - fileIndex;
                rankIndex = 7 - rankIndex;
            }
            return new Vector3(fileIndex - 3.5f, rankIndex - 3.5f, 0f);
        }

        public static int SpriteIndex(PieceType pieceType, PieceColor pieceColor)
            => (int)pieceColor * 6 + ((int)pieceType - 1);
    }
}
