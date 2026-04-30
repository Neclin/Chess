using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;

namespace Chess.Unity
{
    public sealed class BoardView : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject SquarePrefab;
        public GameObject PiecePrefab;

        [Header("Colors")]
        public BoardColorPalette ColorPalette;
        public BoardColorPalette WoodPalette;
        public BoardColorPalette LightPalette;
        public BoardColorPalette DarkPalette;

        [Header("Piece sprites — 12 entries indexed by (color * 6 + (pieceType - 1))")]
        public Sprite[] PieceSprites = new Sprite[12];

        public System.Action<int> OnSquareClicked;
        public bool FlipForBlack;

        private readonly SquareView[] _squareViews = new SquareView[64];
        private readonly Dictionary<int, PieceView> _pieceViewsBySquare = new Dictionary<int, PieceView>();
        private GameObject _draggedPieceObject;

        public SquareView GetSquareView(int squareIndex) => _squareViews[squareIndex];

        private void Awake()
        {
            if (WoodPalette == null) WoodPalette = ColorPalette;
            ColorPalette = SelectPaletteForTheme(SettingsStore.LoadBoardTheme());
        }

        public void Build()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                int fileIndex = Square.FileIndex(squareIndex);
                int rankIndex = Square.RankIndex(squareIndex);
                bool isLightSquare = (fileIndex + rankIndex) % 2 == 1;
                var squareGameObject = Instantiate(SquarePrefab, transform);
                squareGameObject.transform.localPosition = ToLocalPosition(squareIndex);
                squareGameObject.name = $"Sq_{Square.ToAlgebraic(squareIndex)}";
                var squareView = squareGameObject.GetComponent<SquareView>();
                squareView.SquareIndex = squareIndex;
                squareView.IsLightSquare = isLightSquare;
                squareView.ColorPalette = ColorPalette;
                squareView.OnClick = clickedSquareIndex => OnSquareClicked?.Invoke(clickedSquareIndex);
                squareView.OnDragBegin = HandleDragBegin;
                squareView.OnDragMove = HandleDragMove;
                squareView.OnDragEnd = HandleDragEnd;
                squareView.SetHighlightState(SquareHighlightState.Normal);
                _squareViews[squareIndex] = squareView;
            }
        }

        public void Sync(BoardState boardState)
        {
            foreach (var pieceView in _pieceViewsBySquare.Values) Destroy(pieceView.gameObject);
            _pieceViewsBySquare.Clear();
            _draggedPieceObject = null;

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

        public BoardColorPalette SelectPaletteForTheme(BoardTheme theme)
        {
            BoardColorPalette resolvedPalette = theme switch
            {
                BoardTheme.Wood => WoodPalette,
                BoardTheme.Light => LightPalette,
                BoardTheme.Dark => DarkPalette,
                _ => WoodPalette
            };
            return resolvedPalette != null ? resolvedPalette : ColorPalette;
        }

        public void ApplyTheme(BoardTheme theme)
        {
            var resolvedPalette = SelectPaletteForTheme(theme);
            if (resolvedPalette == null) return;
            ColorPalette = resolvedPalette;
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                if (_squareViews[squareIndex] == null) continue;
                _squareViews[squareIndex].ColorPalette = resolvedPalette;
                _squareViews[squareIndex].SetHighlightState(SquareHighlightState.Normal);
            }
        }

        public void SetOrientation(bool flipForBlack)
        {
            if (FlipForBlack == flipForBlack) return;
            FlipForBlack = flipForBlack;
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                if (_squareViews[squareIndex] == null) continue;
                _squareViews[squareIndex].transform.localPosition = ToLocalPosition(squareIndex);
            }
        }

        private void HandleDragBegin(int sourceSquareIndex)
        {
            if (_pieceViewsBySquare.TryGetValue(sourceSquareIndex, out var pieceView))
                _draggedPieceObject = pieceView.gameObject;
            OnSquareClicked?.Invoke(sourceSquareIndex);
        }

        private void HandleDragMove(int sourceSquareIndex, Vector3 worldPosition)
        {
            if (_draggedPieceObject == null) return;
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            _draggedPieceObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y, -0.5f);
        }

        private void HandleDragEnd(int sourceSquareIndex, Vector3 worldPosition)
        {
            var draggedReference = _draggedPieceObject;
            _draggedPieceObject = null;

            int destinationSquareIndex = WorldPositionToSquareIndex(worldPosition);
            if (destinationSquareIndex >= 0 && destinationSquareIndex != sourceSquareIndex)
                OnSquareClicked?.Invoke(destinationSquareIndex);
            else
                OnSquareClicked?.Invoke(sourceSquareIndex);

            if (draggedReference != null)
                draggedReference.transform.localPosition = ToLocalPosition(sourceSquareIndex) + new Vector3(0, 0, -0.1f);
        }

        private int WorldPositionToSquareIndex(Vector3 worldPosition)
        {
            Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
            int fileIndex = Mathf.RoundToInt(localPosition.x + 3.5f);
            int rankIndex = Mathf.RoundToInt(localPosition.y + 3.5f);
            if (FlipForBlack)
            {
                fileIndex = 7 - fileIndex;
                rankIndex = 7 - rankIndex;
            }
            if (fileIndex < 0 || fileIndex > 7 || rankIndex < 0 || rankIndex > 7) return -1;
            return Square.FromFileAndRank(fileIndex, rankIndex);
        }
    }
}
