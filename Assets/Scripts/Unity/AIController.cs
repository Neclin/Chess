using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using Chess.Core.Board;
using Chess.Core.Moves;
using Chess.Core.Search;

namespace Chess.Unity
{
    public sealed class AIController : MonoBehaviour
    {
        public bool UseTimeBudget = false;
        public int TimeBudgetMs = 1000;
        public int MaxDepth = 64;
        public int SearchDepth = 4;

        private readonly ConcurrentQueue<Action> _mainThreadCallbacks = new ConcurrentQueue<Action>();

        private void Update()
        {
            while (_mainThreadCallbacks.TryDequeue(out var callback)) callback();
        }

        public void RequestMove(BoardState liveBoard, Action<Move> onMoveChosen)
        {
            BoardState boardSnapshot = liveBoard.Clone();
            bool useTimeBudget = UseTimeBudget;
            int timeBudgetMs = TimeBudgetMs;
            int maxDepth = MaxDepth;
            int fixedDepth = SearchDepth;

            Task.Run(() =>
            {
                Move chosenMove;
                if (useTimeBudget)
                {
                    IterativeDeepeningResult deepeningResult = IterativeDeepening.Search(boardSnapshot, maxDepth, timeBudgetMs);
                    chosenMove = deepeningResult.BestMove;
                }
                else
                {
                    SearchResult searchResult = MinimaxSearch.FindBestMove(boardSnapshot, fixedDepth);
                    chosenMove = searchResult.BestMove;
                }
                _mainThreadCallbacks.Enqueue(() => onMoveChosen(chosenMove));
            });
        }
    }
}
