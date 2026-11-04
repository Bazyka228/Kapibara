using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Capybara67.Idle {
    public sealed class IdleHold : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IPointerExitHandler {
        public Action pressed,longPressed;public bool Held {get;private set;}
        float heldFor;bool consumed;int pointer;
        public void OnPointerDown(PointerEventData e){if(e.button!=PointerEventData.InputButton.Left||Held)return;pointer=e.pointerId;Held=true;heldFor=0;consumed=false;if(longPressed==null)pressed?.Invoke();}
        void Update(){AdvanceHold(Time.unscaledDeltaTime);}
        // Keep the same production clock, with an explicit step for deterministic gesture checks.
        void AdvanceHold(float dt){if(!Held||longPressed==null||consumed)return;heldFor+=dt;if(heldFor>=.6f){consumed=true;longPressed?.Invoke();}}
        public void OnPointerUp(PointerEventData e){if(!Held||e.pointerId!=pointer)return;bool tap=longPressed!=null&&!consumed;Release();if(tap)pressed?.Invoke();}
        public void OnPointerExit(PointerEventData e){if(e.pointerId==pointer)Release();}
        void OnDisable(){Release();}
        public void Release(){Held=false;consumed=true;}
    }
}
