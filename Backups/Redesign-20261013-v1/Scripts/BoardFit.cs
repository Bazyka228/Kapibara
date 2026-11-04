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
            float scale = Mathf.Min(parent.rect.width / 480f, parent.rect.height / 850f);
            transform.localScale = Vector3.one * Mathf.Max(.01f, scale);
        }
    }
}
