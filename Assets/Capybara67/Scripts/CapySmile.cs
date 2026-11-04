using UnityEngine;
using UnityEngine.UI;
namespace Capybara67
{
    // A small original curved mouth; large mouth states become a clean open oval.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CapySmile : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;float w=r.width,h=r.height;
            if(h>12){
                vh.AddVert(new Vector2(r.center.x,r.center.y),color,Vector2.zero);
                for(int n=0;n<=24;n++){float a=n*Mathf.PI*2/24;vh.AddVert(r.center+new Vector2(Mathf.Cos(a)*w/2,Mathf.Sin(a)*h/2),color,Vector2.zero);if(n>0)vh.AddTriangle(0,n,n+1);}return;
            }
            for(int n=0;n<=16;n++){
                float t=n/16f;float x=r.xMin+t*w,y=r.yMax-4*t*(1-t)*Mathf.Max(6,h);
                vh.AddVert(new Vector2(x,y+1.5f),color,Vector2.zero);vh.AddVert(new Vector2(x,y-1.5f),color,Vector2.zero);
                if(n>0){int i=n*2;vh.AddTriangle(i-2,i-1,i);vh.AddTriangle(i,i-1,i+1);}
            }
        }
    }
}
