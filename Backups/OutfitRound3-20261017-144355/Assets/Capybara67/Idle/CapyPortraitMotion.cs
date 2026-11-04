using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Capybara67.Idle {
    // Deform the existing illustration: no colour patches over the fur and no extra textures.
    [RequireComponent(typeof(Image))]
    public sealed class CapyPortraitMotion : BaseMeshEffect {
        [SerializeField] Vector4[] eyes;
        [SerializeField] float[] eyeWeights;
        float closure;
        float[] columns, rows;
        public float Closure => closure;

        public void Configure(Rect crop, Vector4[] landmarks) {
            eyes = new Vector4[landmarks.Length];
            eyeWeights = new float[landmarks.Length];
            for (int i = 0; i < eyes.Length; i++) {
                var e = landmarks[i];
                eyes[i] = new Vector4((e.x-crop.x)/crop.width, 1-(e.y-crop.y)/crop.height,
                    e.z/crop.width, Mathf.Abs(e.w)*.75f/crop.height);
                eyeWeights[i] = e.w<0 ? .25f : 1;
            }
            columns = Axis(true); rows = Axis(false);
            graphic.SetVerticesDirty();
        }

        float[] Axis(bool horizontal) {
            var values = new SortedSet<float> { 0, 1 };
            foreach (var e in eyes) {
                float centre = horizontal ? e.x : e.y, radius = horizontal ? e.z : e.w;
                for (int n = -12; n <= 12; n++) values.Add(Mathf.Clamp01(centre + radius*n/4f));
            }
            var result = new float[values.Count]; values.CopyTo(result); return result;
        }

        public void SetBlink(float value) {
            value = Mathf.Clamp01(value);
            if (Mathf.Abs(value-closure) < .0001f) return;
            closure = value; graphic.SetVerticesDirty();
        }

        public Vector2 Deform(Vector2 point) {
            if (eyes == null) return point;
            float offset = 0;
            for (int i=0; i<eyes.Length; i++) {
                var e=eyes[i];
                float x = (point.x-e.x)/e.z, y = (point.y-e.y)/e.w;
                // Smooth falloff keeps the muzzle, silhouette and cheeks in place.
                float vertical = Mathf.Abs(y)<=1 ? y : Mathf.Sign(y)*(1-Mathf.SmoothStep(0,1,(Mathf.Abs(y)-1)/2));
                float weight = 1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1.6f,Mathf.Abs(x)));
                offset -= e.w*vertical*closure*.82f*weight*eyeWeights[i];
            }
            return new Vector2(point.x, point.y+offset);
        }

        public override void ModifyMesh(VertexHelper mesh) {
            if (!IsActive() || eyes == null || closure <= .0001f) return;
            if (columns == null) { columns = Axis(true); rows = Axis(false); }
            var image = (Image)graphic;
            var rect = image.GetPixelAdjustedRect();
            var uv = UnityEngine.Sprites.DataUtility.GetOuterUV(image.sprite);
            mesh.Clear();
            foreach (float y in rows) foreach (float x in columns) {
                var p = Deform(new Vector2(x,y));
                mesh.AddVert(new Vector3(rect.x+p.x*rect.width,rect.y+p.y*rect.height),image.color,
                    new Vector2(Mathf.Lerp(uv.x,uv.z,x),Mathf.Lerp(uv.y,uv.w,y)));
            }
            for (int y=0; y<rows.Length-1; y++) for (int x=0; x<columns.Length-1; x++) {
                int a=y*columns.Length+x,b=a+columns.Length;
                mesh.AddTriangle(a,b,b+1); mesh.AddTriangle(b+1,a+1,a);
            }
        }
    }
}
