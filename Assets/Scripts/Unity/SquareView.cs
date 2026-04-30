using UnityEngine;
using UnityEngine.EventSystems;

namespace Chess.Unity
{
    public sealed class SquareView : MonoBehaviour, IPointerClickHandler
    {
        public int SquareIndex;
        public System.Action<int> OnClick;

        public void OnPointerClick(PointerEventData pointerEventData) => OnClick?.Invoke(SquareIndex);
    }
}
