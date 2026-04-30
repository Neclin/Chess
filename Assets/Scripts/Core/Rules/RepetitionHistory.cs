using System.Collections.Generic;

namespace Chess.Core.Rules
{
    public sealed class RepetitionHistory
    {
        private readonly List<ulong> _zobristKeyStack = new List<ulong>();

        public int Count => _zobristKeyStack.Count;

        public void Push(ulong zobristKey) => _zobristKeyStack.Add(zobristKey);

        public void Pop() => _zobristKeyStack.RemoveAt(_zobristKeyStack.Count - 1);

        public void Reset() => _zobristKeyStack.Clear();

        public bool IsThreefold(ulong currentZobristKey)
        {
            int occurrenceCount = 0;
            for (int stackIndex = _zobristKeyStack.Count - 1; stackIndex >= 0; stackIndex--)
            {
                if (_zobristKeyStack[stackIndex] == currentZobristKey)
                {
                    occurrenceCount++;
                    if (occurrenceCount >= 3) return true;
                }
            }
            return false;
        }
    }
}
