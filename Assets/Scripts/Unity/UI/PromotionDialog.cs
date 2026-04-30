using UnityEngine;
using UnityEngine.UI;
using Chess.Core.Board;

namespace Chess.Unity.UI
{
    public sealed class PromotionDialog : MonoBehaviour
    {
        public GameObject Root;
        public Button QueenButton;
        public Button RookButton;
        public Button BishopButton;
        public Button KnightButton;
        public Image[] Icons = new Image[4];
        public Sprite[] WhitePieceIcons = new Sprite[4];
        public Sprite[] BlackPieceIcons = new Sprite[4];

        private System.Action<PieceType> _onPromotionChosen;

        private void Awake()
        {
            Root.SetActive(false);
            QueenButton.onClick.AddListener(() => Choose(PieceType.Queen));
            RookButton.onClick.AddListener(() => Choose(PieceType.Rook));
            BishopButton.onClick.AddListener(() => Choose(PieceType.Bishop));
            KnightButton.onClick.AddListener(() => Choose(PieceType.Knight));
        }

        public void Show(PieceColor pieceColor, System.Action<PieceType> onChosen)
        {
            _onPromotionChosen = onChosen;
            for (int iconIndex = 0; iconIndex < 4; iconIndex++)
                Icons[iconIndex].sprite = pieceColor == PieceColor.White ? WhitePieceIcons[iconIndex] : BlackPieceIcons[iconIndex];
            Root.SetActive(true);
        }

        private void Choose(PieceType chosenPieceType)
        {
            Root.SetActive(false);
            _onPromotionChosen?.Invoke(chosenPieceType);
        }
    }
}
