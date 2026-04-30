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
        public GameOverDialog GameOverUI;
        public AIController AI;

        public string StartingFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        public PieceColor HumanColor = PieceColor.White;

        public BoardState State { get; private set; }
        public RepetitionHistory History { get; } = new RepetitionHistory();
        public BoardState InitialBoardState { get; private set; }
        public IReadOnlyList<Move> MovesPlayed => _movesPlayed;
        public GameResult LastResult { get; private set; }

        private readonly List<Move> _movesPlayed = new List<Move>();
        private Move? _lastMovePlayed;

        private void Start()
        {
            ApplyMainMenuPreferences();
            Board.Build();
            InitialiseGame();

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

            if (State.SideToMove != HumanColor) AI.RequestMove(State, ApplyAiMove);
        }

        public IReadOnlyList<Move> LegalMovesFromSquare(int squareIndex)
        {
            if (State.SideToMove != HumanColor) return System.Array.Empty<Move>();
            var allLegalMoves = new List<Move>();
            MoveGenerator.GenerateLegal(State, allLegalMoves);
            return allLegalMoves.FindAll(legalMove => legalMove.FromSquare == squareIndex);
        }

        public void Restart(PieceColor humanColorForNextGame)
        {
            HumanColor = humanColorForNextGame;
            InitialiseGame();
            if (State.SideToMove != HumanColor) AI.RequestMove(State, ApplyAiMove);
        }

        private void ApplyMainMenuPreferences()
        {
            PieceColor? preferredHumanColor = GamePreferences.LoadHumanColor();
            if (preferredHumanColor.HasValue) HumanColor = preferredHumanColor.Value;
        }

        private void InitialiseGame()
        {
            State = FenParser.Parse(StartingFen);
            InitialBoardState = State.Clone();
            History.Reset();
            History.Push(State.ZobristKey);
            _movesPlayed.Clear();
            _lastMovePlayed = null;
            LastResult = GameResult.Ongoing;
            Board.Sync(State);
            Highlights.Clear();
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
                    if (LastResult == GameResult.Ongoing && State.SideToMove != HumanColor) AI.RequestMove(State, ApplyAiMove);
                });
                return;
            }
            Apply(chosenMove);
            if (LastResult == GameResult.Ongoing && State.SideToMove != HumanColor) AI.RequestMove(State, ApplyAiMove);
        }

        private void ApplyAiMove(Move aiMove) => Apply(aiMove);

        private void Apply(Move appliedMove)
        {
            MoveExecutor.MakeMove(State, appliedMove);
            History.Push(State.ZobristKey);
            _movesPlayed.Add(appliedMove);
            _lastMovePlayed = appliedMove;
            Board.Sync(State);
            Highlights.Clear();
            Highlights.ShowLastMove(appliedMove.FromSquare, appliedMove.ToSquare);
            CheckGameOver();
        }

        private void CheckGameOver()
        {
            GameResult result = GameStateChecker.Evaluate(State);
            LastResult = result;
            if (result == GameResult.Ongoing) return;
            if (GameOverUI != null) GameOverUI.Show(this);
            else Debug.Log($"Game over: {result}");
        }
    }
}
