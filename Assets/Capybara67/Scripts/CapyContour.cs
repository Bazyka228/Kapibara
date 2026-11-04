using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Capybara67
{
    // Original silhouette, authored as a closed polygon in top-left coordinates.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CapyContour : MaskableGraphic
    {
        public Vector2[] points;
        public Vector2 sourceSize = new Vector2(240,150);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(points==null || points.Length<3)return;
            var r=rectTransform.rect;var indices=new List<int>();
            for(int n=0;n<points.Length;n++){
                vh.AddVert(new Vector2(r.xMin+points[n].x/sourceSize.x*r.width,r.yMax-points[n].y/sourceSize.y*r.height),color,Vector2.zero);indices.Add(n);
            }
            float area=0;for(int n=0;n<points.Length;n++){var a=points[n];var b=points[(n+1)%points.Length];area+=a.x*b.y-b.x*a.y;}
            float sign=area>0?1:-1;int guard=points.Length*points.Length;
            while(indices.Count>2 && guard-->0){bool clipped=false;
                for(int n=0;n<indices.Count;n++){
                    int a=indices[(n+indices.Count-1)%indices.Count],b=indices[n],c=indices[(n+1)%indices.Count];
                    if(Cross(points[b]-points[a],points[c]-points[b])*sign<=.0001f)continue;
                    bool inside=false;foreach(int k in indices)if(k!=a && k!=b && k!=c && InTriangle(points[k],points[a],points[b],points[c],sign)){inside=true;break;}
                    if(inside)continue;vh.AddTriangle(a,b,c);indices.RemoveAt(n);clipped=true;break;
                }if(!clipped)break;
            }
            // A transparent one-pixel fringe keeps the vector outline soft at phone resolution.
            float aa=.7f/Mathf.Max(.1f,Mathf.Abs(rectTransform.lossyScale.x));Color clear=color;clear.a=0;
            for(int n=0;n<points.Length;n++){
                var prev=points[(n+points.Length-1)%points.Length];var here=points[n];var next=points[(n+1)%points.Length];
                Vector2 before=new Vector2((here.x-prev.x)*r.width/sourceSize.x,-(here.y-prev.y)*r.height/sourceSize.y).normalized;
                Vector2 after=new Vector2((next.x-here.x)*r.width/sourceSize.x,-(next.y-here.y)*r.height/sourceSize.y).normalized;
                Vector2 normal=(new Vector2(-before.y,before.x)+new Vector2(-after.y,after.x)).normalized*sign;
                Vector2 pos=new Vector2(r.xMin+here.x/sourceSize.x*r.width,r.yMax-here.y/sourceSize.y*r.height);
                vh.AddVert(pos+normal*aa,clear,Vector2.zero);
            }
            for(int n=0;n<points.Length;n++){int next=(n+1)%points.Length;vh.AddTriangle(n,next,n+points.Length);vh.AddTriangle(next,next+points.Length,n+points.Length);}
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static bool InTriangle(Vector2 p,Vector2 a,Vector2 b,Vector2 c,float s)=>Cross(b-a,p-a)*s>=0 && Cross(c-b,p-b)*s>=0 && Cross(a-c,p-c)*s>=0;
    }
}
