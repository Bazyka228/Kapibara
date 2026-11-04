using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Capybara67.Idle {
    public sealed class IdleHold : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
        public Action pressed;public bool Held {get;private set;}
        public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left)return;Held=true;pressed?.Invoke();}
        public void OnPointerUp(PointerEventData e){Held=false;}
        public void OnPointerExit(PointerEventData e){Held=false;}
        void OnDisable(){Held=false;}
        public void Release(){Held=false;}
    }
}
