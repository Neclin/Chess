using Chess.Core.Moves;

namespace Chess.Core.Search
{
    public enum TranspositionBound : byte { Exact, LowerBound, UpperBound }

    public struct TranspositionEntry
    {
        public ulong Key;
        public Move BestMove;
        public short Depth;
        public int Score;
        public TranspositionBound Bound;
    }

    public sealed class TranspositionTable
    {
        private readonly TranspositionEntry[] _entries;
        private readonly ulong _indexMask;
        private int _occupiedEntryCount;

        public int EntryCount => _entries.Length;
        public int OccupiedEntryCount => _occupiedEntryCount;

        public TranspositionTable(int sizeMegabytes)
        {
            const int approximateBytesPerEntry = 24;
            int targetEntryCount = sizeMegabytes * 1024 * 1024 / approximateBytesPerEntry;
            int powerOfTwoEntryCount = 1;
            while (powerOfTwoEntryCount * 2 <= targetEntryCount) powerOfTwoEntryCount *= 2;
            _entries = new TranspositionEntry[powerOfTwoEntryCount];
            _indexMask = (ulong)(powerOfTwoEntryCount - 1);
        }

        public void Store(ulong key, int depth, int score, TranspositionBound bound, Move bestMove)
        {
            ref var slot = ref _entries[key & _indexMask];
            if (slot.Key == 0UL) _occupiedEntryCount++;
            slot.Key = key;
            slot.Depth = (short)depth;
            slot.Score = score;
            slot.Bound = bound;
            slot.BestMove = bestMove;
        }

        public bool Probe(ulong key, out TranspositionEntry entry)
        {
            entry = _entries[key & _indexMask];
            return entry.Key == key;
        }

        public void Clear()
        {
            System.Array.Clear(_entries, 0, _entries.Length);
            _occupiedEntryCount = 0;
        }

        public double FillPercent()
        {
            if (_entries.Length == 0) return 0.0;
            return 100.0 * _occupiedEntryCount / _entries.Length;
        }
    }
}
