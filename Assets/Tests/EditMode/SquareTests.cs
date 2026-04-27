using NUnit.Framework;
using Chess.Core.Board;

namespace Chess.Core.Tests
{
    public class SquareTests
    {
        [Test] public void A1IsZero() => Assert.AreEqual(0, Square.FromAlgebraic("a1"));
        [Test] public void H1IsSeven() => Assert.AreEqual(7, Square.FromAlgebraic("h1"));
        [Test] public void A8IsFiftySix() => Assert.AreEqual(56, Square.FromAlgebraic("a8"));
        [Test] public void H8IsSixtyThree() => Assert.AreEqual(63, Square.FromAlgebraic("h8"));
        [Test] public void RoundTrip() => Assert.AreEqual("e4", Square.ToAlgebraic(Square.FromAlgebraic("e4")));
        [Test] public void FileOfE4Is4() => Assert.AreEqual(4, Square.FileIndex(Square.FromAlgebraic("e4")));
        [Test] public void RankOfE4Is3() => Assert.AreEqual(3, Square.RankIndex(Square.FromAlgebraic("e4")));
    }
}
