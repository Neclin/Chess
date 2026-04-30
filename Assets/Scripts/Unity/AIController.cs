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
        public int SearchDepth = 4;

        private readonly ConcurrentQueue<Action> _mainThreadCallbacks = new ConcurrentQueue<Action>();

        private void Update()
        {
            while (_mainThreadCallbacks.TryDequeue(out var callback)) callback();
        }

        public void RequestMove(BoardState liveBoard, Action<Move> onMoveChosen)
        {
            BoardState boardSnapshot = liveBoard.Clone();
            int depth = SearchDepth;
            Task.Run(() =>
            {
                SearchResult searchResult = MinimaxSearch.FindBestMove(boardSnapshot, depth);
                _mainThreadCallbacks.Enqueue(() => onMoveChosen(searchResult.BestMove));
            });
        }
    }
}
