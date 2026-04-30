using NUnit.Framework;
using Chess.Core.Rules;

namespace Chess.Core.Tests
{
    public class RepetitionHistoryTests
    {
        [Test]
        public void EmptyHistory_IsNotThreefold()
        {
            var history = new RepetitionHistory();
            Assert.IsFalse(history.IsThreefold(0xDEADBEEFUL));
        }

        [Test]
        public void SecondOccurrence_IsNotThreefold()
        {
            var history = new RepetitionHistory();
            history.Push(0xDEADBEEFUL);
            history.Push(0x1UL);
            history.Push(0xDEADBEEFUL);
            history.Push(0x2UL);
            Assert.IsFalse(history.IsThreefold(0xDEADBEEFUL));
        }

        [Test]
        public void ThirdOccurrence_IsThreefold()
        {
            var history = new RepetitionHistory();
            history.Push(0xDEADBEEFUL);
            history.Push(0x1UL);
            history.Push(0xDEADBEEFUL);
            history.Push(0x2UL);
            history.Push(0xDEADBEEFUL);
            Assert.IsTrue(history.IsThreefold(0xDEADBEEFUL));
        }

        [Test]
        public void DifferentKey_IsNotThreefold_WhenOtherKeyRepeats()
        {
            var history = new RepetitionHistory();
            history.Push(0xDEADBEEFUL);
            history.Push(0xDEADBEEFUL);
            history.Push(0xDEADBEEFUL);
            Assert.IsFalse(history.IsThreefold(0xCAFEBABEUL));
        }

        [Test]
        public void Pop_RemovesMostRecentEntry()
        {
            var history = new RepetitionHistory();
            history.Push(0xDEADBEEFUL);
            history.Push(0xDEADBEEFUL);
            history.Push(0xDEADBEEFUL);
            history.Pop();
            Assert.IsFalse(history.IsThreefold(0xDEADBEEFUL));
        }

        [Test]
        public void Reset_ClearsAllEntries()
        {
            var history = new RepetitionHistory();
            history.Push(0xDEADBEEFUL);
            history.Push(0xDEADBEEFUL);
            history.Push(0xDEADBEEFUL);
            history.Reset();
            Assert.AreEqual(0, history.Count);
            Assert.IsFalse(history.IsThreefold(0xDEADBEEFUL));
        }
    }
}
