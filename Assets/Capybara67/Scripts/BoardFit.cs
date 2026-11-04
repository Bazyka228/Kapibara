using UnityEngine;

namespace Capybara67
{
    [ExecuteAlways]
    public sealed class BoardFit : MonoBehaviour
    {
        void LateUpdate()
        {
            var parent = transform.parent as RectTransform;
            if (parent == null) return;
            Rect safe=Screen.safeArea;
            if(safe.width<=0 || safe.height<=0)safe=new Rect(0,0,Screen.width,Screen.height);
            float xRatio=parent.rect.width/Mathf.Max(1,Screen.width), yRatio=parent.rect.height/Mathf.Max(1,Screen.height);
            float scale = Mathf.Min(safe.width*xRatio / 480f, safe.height*yRatio / 850f);
            transform.localScale = Vector3.one * Mathf.Max(.01f, scale);
            ((RectTransform)transform).anchoredPosition=new Vector2((safe.center.x-Screen.width*.5f)*xRatio,(safe.center.y-Screen.height*.5f)*yRatio);
        }
    }
}
