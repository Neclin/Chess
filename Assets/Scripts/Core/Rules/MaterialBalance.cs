using System.Collections.Generic;
using Chess.Core.Board;

namespace Chess.Core.Rules
{
    public readonly struct CapturedPiece
    {
        public readonly PieceType PieceType;
        public readonly PieceColor CapturerColor;

        public CapturedPiece(PieceType pieceType, PieceColor capturerColor)
        {
            PieceType = pieceType;
            CapturerColor = capturerColor;
        }
    }

    public static class MaterialBalance
    {
        public static int Value(PieceType pieceType) => pieceType switch
        {
            PieceType.Pawn => 1,
            PieceType.Knight => 3,
            PieceType.Bishop => 3,
            PieceType.Rook => 5,
            PieceType.Queen => 9,
            _ => 0
        };

        public static int WhiteMinusBlackScore(IReadOnlyList<CapturedPiece> capturedPieces)
        {
            if (capturedPieces == null) return 0;
            int signedScore = 0;
            for (int captureIndex = 0; captureIndex < capturedPieces.Count; captureIndex++)
            {
                var capturedPiece = capturedPieces[captureIndex];
                int valueOfCapturedPiece = Value(capturedPiece.PieceType);
                signedScore += capturedPiece.CapturerColor == PieceColor.White ? valueOfCapturedPiece : -valueOfCapturedPiece;
            }
            return signedScore;
        }
    }
}
