using UnityEngine;

namespace Chess.Unity
{
    public enum SquareHighlightState
    {
        Normal,
        MoveTarget,
        CaptureTarget,
        Selected,
        LastMove
    }

    [CreateAssetMenu(fileName = "BoardColorPalette", menuName = "Chess/Board Color Palette")]
    public sealed class BoardColorPalette : ScriptableObject
    {
        [Header("Light squares")]
        public Color LightNormal = new Color(0.94f, 0.85f, 0.71f);
        public Color LightMoveTarget = new Color(0.62f, 0.82f, 0.52f);
        public Color LightCaptureTarget = new Color(0.92f, 0.55f, 0.50f);
        public Color LightSelected = new Color(0.95f, 0.85f, 0.32f);
        public Color LightLastMove = new Color(0.96f, 0.91f, 0.58f);

        [Header("Dark squares")]
        public Color DarkNormal = new Color(0.71f, 0.53f, 0.39f);
        public Color DarkMoveTarget = new Color(0.42f, 0.62f, 0.36f);
        public Color DarkCaptureTarget = new Color(0.70f, 0.40f, 0.32f);
        public Color DarkSelected = new Color(0.78f, 0.58f, 0.20f);
        public Color DarkLastMove = new Color(0.78f, 0.66f, 0.36f);

        public Color GetColor(bool isLightSquare, SquareHighlightState highlightState)
        {
            if (isLightSquare)
            {
                return highlightState switch
                {
                    SquareHighlightState.MoveTarget => LightMoveTarget,
                    SquareHighlightState.CaptureTarget => LightCaptureTarget,
                    SquareHighlightState.Selected => LightSelected,
                    SquareHighlightState.LastMove => LightLastMove,
                    _ => LightNormal
                };
            }
            return highlightState switch
            {
                SquareHighlightState.MoveTarget => DarkMoveTarget,
                SquareHighlightState.CaptureTarget => DarkCaptureTarget,
                SquareHighlightState.Selected => DarkSelected,
                SquareHighlightState.LastMove => DarkLastMove,
                _ => DarkNormal
            };
        }
    }
}
