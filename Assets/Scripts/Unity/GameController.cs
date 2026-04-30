using System.Collections.Generic;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Rules;
using Chess.Unity.UI;

namespace Chess.Unity
{
    public sealed class GameController : MonoBehaviour
    {
        public BoardView Board;
        public InputController Input;
        public HighlightOverlay Highlights;
        public PromotionDialog PromotionUI;

        public string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public BoardState State { get; private set; }
        public RepetitionHistory History { get; } = new RepetitionHistory();

        private Move? _lastMovePlayed;

        private void Start()
        {
            Board.Build();
            State = FenParser.Parse(StartingFen);
            Board.Sync(State);

            Input.GetLegalMovesFromSquare = LegalMovesFromSquare;
            Input.OnSelectionChanged = (selectedSquareIndex, legalMovesFromSelection) =>
            {
                Highlights.Clear();
                if (_lastMovePlayed.HasValue) Highlights.ShowLastMove(_lastMovePlayed.Value.FromSquare, _lastMovePlayed.Value.ToSquare);
                Highlights.ShowSelection(selectedSquareIndex);
                foreach (var legalMove in legalMovesFromSelection) Highlights.ShowMoveTarget(legalMove.ToSquare, legalMove.IsCapture);
            };
            Input.OnSelectionCleared = () =>
            {
                Highlights.Clear();
                if (_lastMovePlayed.HasValue) Highlights.ShowLastMove(_lastMovePlayed.Value.FromSquare, _lastMovePlayed.Value.ToSquare);
            };
            Input.OnMoveChosen = ApplyChosenMove;
        }

        public IReadOnlyList<Move> LegalMovesFromSquare(int squareIndex)
        {
            var allLegalMoves = new List<Move>();
            MoveGenerator.GenerateLegal(State, allLegalMoves);
            return allLegalMoves.FindAll(legalMove => legalMove.FromSquare == squareIndex);
        }

        private void ApplyChosenMove(Move chosenMove)
        {
            if (chosenMove.IsPromotion)
            {
                PieceColor promotingColor = State.SideToMove;
                PromotionUI.Show(promotingColor, chosenPromotionType =>
                {
                    var promotionMove = new Move(chosenMove.FromSquare, chosenMove.ToSquare, chosenMove.Flags, chosenPromotionType);
                    Apply(promotionMove);
                });
                return;
            }
            Apply(chosenMove);
        }

        private void Apply(Move appliedMove)
        {
            MoveExecutor.MakeMove(State, appliedMove);
            History.Push(State.ZobristKey);
            _lastMovePlayed = appliedMove;
            Board.Sync(State);
            Highlights.Clear();
            Highlights.ShowLastMove(appliedMove.FromSquare, appliedMove.ToSquare);
            CheckGameOver();
        }

        private void CheckGameOver()
        {
            var result = GameStateChecker.Evaluate(State);
            if (result == GameResult.Ongoing) return;
            Debug.Log($"Game over: {result}");
        }
    }
}
