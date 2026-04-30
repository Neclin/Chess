using NUnit.Framework;
using Chess.Core.Moves;
using Chess.Core.Search;

namespace Chess.Core.Tests
{
    public class TranspositionTableTests
    {
        [Test]
        public void StoreThenProbe_ReturnsStoredEntryForSameKey()
        {
            var table = new TranspositionTable(sizeMegabytes: 1);
            var bestMove = new Move(0, 1);
            table.Store(0xDEADBEEFUL, depth: 3, score: 42, bound: TranspositionBound.Exact, bestMove: bestMove);

            Assert.IsTrue(table.Probe(0xDEADBEEFUL, out var storedEntry));
            Assert.AreEqual(42, storedEntry.Score);
            Assert.AreEqual(3, storedEntry.Depth);
            Assert.AreEqual(TranspositionBound.Exact, storedEntry.Bound);
            Assert.AreEqual(bestMove.FromSquare, storedEntry.BestMove.FromSquare);
            Assert.AreEqual(bestMove.ToSquare, storedEntry.BestMove.ToSquare);
        }

        [Test]
        public void Probe_ReturnsFalseForDifferentKey()
        {
            var table = new TranspositionTable(sizeMegabytes: 1);
            table.Store(1UL, depth: 1, score: 1, bound: TranspositionBound.Exact, bestMove: default);
            Assert.IsFalse(table.Probe(2UL, out _));
        }

        [Test]
        public void Clear_DropsAllEntries()
        {
            var table = new TranspositionTable(sizeMegabytes: 1);
            table.Store(0xCAFEUL, depth: 1, score: 1, bound: TranspositionBound.Exact, bestMove: default);
            table.Clear();
            Assert.IsFalse(table.Probe(0xCAFEUL, out _));
        }

        [Test]
        public void EntryCount_IsPowerOfTwo()
        {
            var table = new TranspositionTable(sizeMegabytes: 4);
            int entryCount = table.EntryCount;
            Assert.That(entryCount, Is.GreaterThan(0));
            Assert.AreEqual(0, entryCount & (entryCount - 1), "entry count must be a power of two");
        }
    }
}
