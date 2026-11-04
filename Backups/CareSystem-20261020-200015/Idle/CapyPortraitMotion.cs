using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;

namespace Capybara67.Idle {
    // Eyelids change only ink inside the eye regions. The portrait stays an untouched quad.
    [RequireComponent(typeof(Image))]
    public sealed class CapyPortraitMotion : MonoBehaviour {
        Image portrait;
        Material blinkMaterial, previousMaterial;
        Vector4[] regions = System.Array.Empty<Vector4>();
        float closure;
        int skinArt; CapyIdleGame game; float skinTime;
        static readonly int BlinkId = Shader.PropertyToID("_Blink");
        public float Closure => closure;
        public int AnimatedEyeCount => regions.Length;

        public void Configure(Rect crop, Vector4[] landmarks) {
            portrait = GetComponent<Image>();
            previousMaterial = portrait.material;
            var shader = Resources.Load<Shader>("Shaders/CapyEyeBlink");
            if (!shader || !shader.isSupported) throw new System.InvalidOperationException("Capy eye shader unavailable");
            blinkMaterial = new Material(shader) { name = "Capy eyes only", hideFlags = HideFlags.DontSave };
            var openEyes = new List<Vector4>();
            var uv = DataUtility.GetOuterUV(portrait.sprite);
            foreach (var eye in landmarks) {
                // Closed eyes in the drawing are already relaxed. Never flatten or move them.
                if (eye.w <= 0) continue;
                if (openEyes.Count == 2) throw new System.InvalidOperationException("More than two portrait eyes");
                var region = new Vector4((eye.x-crop.x)/crop.width, 1-(eye.y-crop.y)/crop.height,
                    eye.z*.8f/crop.width, eye.w*.8f/crop.height);
                openEyes.Add(region);
                blinkMaterial.SetVector("_Eye"+(openEyes.Count-1), new Vector4(
                    Mathf.Lerp(uv.x,uv.z,region.x), Mathf.Lerp(uv.y,uv.w,region.y),
                    region.z*(uv.z-uv.x), region.w*(uv.w-uv.y)));
            }
            regions = openEyes.ToArray();
            blinkMaterial.SetFloat("_EyeCount",regions.Length);
            portrait.material = blinkMaterial;
            var uvBounds=DataUtility.GetOuterUV(portrait.sprite);
            blinkMaterial.SetVector("_PortraitUV",uvBounds);
            game=GetComponentInParent<CapyIdleGame>();
            SetBlink(0);
        }

        public int SkinArt=>skinArt;
        public void SetWink(float amount){SetProperty("_Wink",Mathf.Clamp01(amount));}
        public void SetSkin(int art){skinArt=Mathf.Clamp(art,0,9);SetProperty("_Skin",skinArt==0?10:skinArt);}
        public void SetOutfitSkin(int art,Texture2D mask){
            skinArt=Mathf.Clamp(art,0,9);
            blinkMaterial.SetTexture("_FurMask",mask);blinkMaterial.SetFloat("_UseFurMask",1);
            // "Classic" restores the illustrator's original fur, without recolouring cloth.
            SetProperty("_Skin",skinArt);
            var rendered=portrait.materialForRendering;
            if(rendered&&rendered!=blinkMaterial){rendered.SetTexture("_FurMask",mask);rendered.SetFloat("_UseFurMask",1);}
        }
        void SetProperty(string key,float value){if(!blinkMaterial)return;blinkMaterial.SetFloat(key,value);var rendered=portrait.materialForRendering;if(rendered&&rendered!=blinkMaterial)rendered.SetFloat(key,value);}
        void Update(){if(skinArt==0||!game||game.Model==null)return;if(game.CosmeticReduced){SetProperty("_SkinTime",0);return;}if(!game.CosmeticMotionAllowed)return;skinTime+=Mathf.Min(Time.unscaledDeltaTime,.1f);SetProperty("_SkinTime",skinTime);}

        public bool IsEyePixel(Vector2 normalized, Vector2 padding) {
            foreach (var eye in regions)
                if (Mathf.Abs(normalized.x-eye.x)<=eye.z+padding.x && Mathf.Abs(normalized.y-eye.y)<=eye.w+padding.y) return true;
            return false;
        }

        public void SetBlink(float value) {
            closure = Mathf.Clamp01(value);
            if (!blinkMaterial) return;
            blinkMaterial.SetFloat(BlinkId,closure);
            // UI masks may use a stencil copy of the material.
            var rendered = portrait.materialForRendering;
            if (rendered && rendered != blinkMaterial) rendered.SetFloat(BlinkId,closure);
        }

        void OnDestroy() {
            if (portrait && portrait.material == blinkMaterial) portrait.material = previousMaterial;
            if (blinkMaterial) Destroy(blinkMaterial);
        }
    }
}
