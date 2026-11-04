using UnityEngine;
using UnityEngine.UI;

namespace Capybara67
{
    // Resolution-independent flat illustration. All shapes remain editable in the scene.
    [AddComponentMenu("Capybara 67/Soft Shape")]
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SoftShape : MaskableGraphic
    {
        public bool ellipse;
        public float radius = 16;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            Vector2 center = r.center;
            vh.AddVert(center, color, Vector2.zero);
            const int segments = 64;
            float corner = Mathf.Min(radius, Mathf.Min(r.width, r.height) / 2);
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float c = Mathf.Cos(a), s = Mathf.Sin(a);
                Vector2 p;
                if (ellipse) p = center + new Vector2(c * r.width / 2, s * r.height / 2);
                else p = center + new Vector2(Mathf.Sign(c) * (r.width / 2 - corner) + c * corner,
                    Mathf.Sign(s) * (r.height / 2 - corner) + s * corner);
                vh.AddVert(p, color, Vector2.zero);
                if (i > 0) vh.AddTriangle(0, i, i + 1);
            }
        }
    }
}
