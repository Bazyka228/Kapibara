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
        // Opt in only for UI and scenery. Character and outfit geometry stays on the original path.
        public bool polished;
        public float lightStrength = .2f;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = rectTransform.rect;
            if(r.width<=0 || r.height<=0)return;
            if(polished){PopulatePolished(vh,r);return;}
            Vector2 center = r.center;
            vh.AddVert(center, color, Vector2.zero);
            const int segments = 64;
            float corner = Mathf.Min(radius, Mathf.Min(r.width, r.height) / 2);
            float aa=Mathf.Min(.75f/Mathf.Max(.1f,Mathf.Abs(rectTransform.lossyScale.x)),Mathf.Min(r.width,r.height)*.3f);
            Color clear=color;clear.a=0;
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float c = Mathf.Cos(a), s = Mathf.Sin(a);
                Vector2 p;
                if (ellipse) p = center + new Vector2(c * r.width / 2, s * r.height / 2);
                else p = center + new Vector2(Mathf.Sign(c) * (r.width / 2 - corner) + c * corner,
                    Mathf.Sign(s) * (r.height / 2 - corner) + s * corner);
                Vector2 normal=ellipse?new Vector2(c/Mathf.Max(1,r.width),s/Mathf.Max(1,r.height)).normalized:new Vector2(c,s);
                vh.AddVert(p-normal*aa*.5f, color, Vector2.zero);
                vh.AddVert(p+normal*aa*.5f, clear, Vector2.zero);
                if(i>0){int inner=1+i*2,outer=inner+1;vh.AddTriangle(0,inner-2,inner);vh.AddTriangle(inner-2,outer-2,inner);vh.AddTriangle(inner,outer-2,outer);}
            }
        }
        void PopulatePolished(VertexHelper vh,Rect r){
            const int segments=48;
            Vector2 center=r.center;
            float corner=Mathf.Min(radius,Mathf.Min(r.width,r.height)/2);
            float aa=Mathf.Min(.75f/Mathf.Max(.1f,Mathf.Abs(rectTransform.lossyScale.x)),Mathf.Min(r.width,r.height)*.3f);
            vh.AddVert(center,Lit(center,r),Vector2.zero);
            // Interior rings give the light a soft falloff instead of a flat triangle fan.
            for(int ring=0;ring<4;ring++)for(int i=0;i<segments;i++){
                float a=i*Mathf.PI*2/segments,c=Mathf.Cos(a),s=Mathf.Sin(a);
                Vector2 edge=ellipse?new Vector2(c*r.width/2,s*r.height/2):new Vector2(Mathf.Sign(c)*(r.width/2-corner)+c*corner,Mathf.Sign(s)*(r.height/2-corner)+s*corner);
                Vector2 normal=ellipse?new Vector2(c/Mathf.Max(1,r.width),s/Mathf.Max(1,r.height)).normalized:new Vector2(c,s);
                Vector2 pos=center+edge*(ring==0?.45f:ring==1?.8f:1)+(ring==2?-normal*aa*.5f:ring==3?normal*aa*.5f:Vector2.zero);
                Color tint=Lit(pos,r);if(ring==3)tint.a=0;
                vh.AddVert(pos,tint,Vector2.zero);
                int next=(i+1)%segments,index=1+ring*segments+i;
                if(ring==0)vh.AddTriangle(0,index,1+next);
                else {int previous=index-segments,previousNext=1+(ring-1)*segments+next;vh.AddTriangle(previous,index,previousNext);vh.AddTriangle(index,1+ring*segments+next,previousNext);}
            }
        }
        Color Lit(Vector2 p,Rect r){
            float x=(p.x-r.xMin)/r.width,y=(p.y-r.yMin)/r.height;
            // Shared illustrated material: warm edge light, rounded shade and a broad satin highlight.
            // Flat/protected character geometry never calls this branch.
            float dome=Mathf.Clamp01(1-Mathf.Pow((x-.46f)*1.65f,2)-Mathf.Pow((y-.55f)*1.3f,2));
            float light=(y-.49f)*.85f+(dome-.58f)*.3f;
            float shine=Mathf.Exp(-((x-.25f)*(x-.25f)*5+(y-.82f)*(y-.82f)*17));
            Color result=Color.Lerp(color,new Color(.20f,.10f,.035f,color.a),Mathf.Max(0,-light)*lightStrength*.75f);
            result=Color.Lerp(result,new Color(1,.97f,.79f,color.a),(Mathf.Max(0,light)*.55f+shine*.48f)*lightStrength);
            result.a=color.a;return result;
        }
    }
}
