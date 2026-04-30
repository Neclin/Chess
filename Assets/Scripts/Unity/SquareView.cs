using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess.Unity
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SquareView : MonoBehaviour,
        IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public int SquareIndex;
        public bool IsLightSquare;
        public BoardColorPalette ColorPalette;

        public System.Action<int> OnClick;
        public System.Action<int> OnDragBegin;
        public System.Action<int, Vector3> OnDragMove;
        public System.Action<int, Vector3> OnDragEnd;

        private SpriteRenderer _spriteRenderer;

        private void Awake() => _spriteRenderer = GetComponent<SpriteRenderer>();

        public void SetHighlightState(SquareHighlightState highlightState)
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (ColorPalette == null) return;
            _spriteRenderer.color = ColorPalette.GetColor(IsLightSquare, highlightState);
        }

        public void OnPointerClick(PointerEventData pointerEventData) => OnClick?.Invoke(SquareIndex);

        public void OnBeginDrag(PointerEventData pointerEventData) => OnDragBegin?.Invoke(SquareIndex);

        public void OnDrag(PointerEventData pointerEventData)
            => OnDragMove?.Invoke(SquareIndex, ScreenPositionToWorldPosition(pointerEventData.position));

        public void OnEndDrag(PointerEventData pointerEventData)
            => OnDragEnd?.Invoke(SquareIndex, ScreenPositionToWorldPosition(pointerEventData.position));

        private static Vector3 ScreenPositionToWorldPosition(Vector2 screenPosition)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null) return Vector3.zero;
            float depthFromCamera = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, depthFromCamera));
        }
    }
}
