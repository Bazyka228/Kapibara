Shader "Capybara67/UI Eyes Only" {
 Properties {
  [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
  _Color ("Tint", Color) = (1,1,1,1)
  _Blink ("Eye closure", Range(0,1)) = 0
  _EyeCount ("Open eyes", Float) = 0
  _Eye0 ("First eye UV", Vector) = (0,0,0,0)
  _Eye1 ("Second eye UV", Vector) = (0,0,0,0)
  _StencilComp ("Stencil Comparison", Float) = 8
  _Stencil ("Stencil ID", Float) = 0
  _StencilOp ("Stencil Operation", Float) = 0
  _StencilWriteMask ("Stencil Write Mask", Float) = 255
  _StencilReadMask ("Stencil Read Mask", Float) = 255
  _ColorMask ("Color Mask", Float) = 15
  [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
 }
 SubShader {
  Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
  Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha
  ColorMask [_ColorMask]
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct appdata { float4 vertex:POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct v2f { float4 vertex:SV_POSITION; float4 color:COLOR; float2 uv:TEXCOORD0; float4 world:TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
   sampler2D _MainTex;
   float4 _Color, _TextureSampleAdd, _ClipRect, _Eye0, _Eye1;
   float _Blink, _EyeCount;
   v2f vert(appdata input) {
    v2f output; UNITY_SETUP_INSTANCE_ID(input); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.world=input.vertex; output.vertex=UnityObjectToClipPos(input.vertex);
    output.uv=input.uv; output.color=input.color*_Color; return output;
   }
   float ink(float4 texel) { return (1-smoothstep(.16,.32,max(texel.r,max(texel.g,texel.b))))*step(.95,texel.a); }
   float4 skinSample(float2 uv) {
    float4 c=tex2D(_MainTex,uv); float weight=step(.95,c.a)*(1-ink(c));
    return float4(c.rgb*weight,weight);
   }
   float3 closeEye(float2 uv,float4 original,float4 eye) {
    float2 eyeOffset=(uv-eye.xy)/eye.zw;
    if (max(abs(eyeOffset.x),abs(eyeOffset.y))>=1) return original.rgb;
    // Only the dark eye ink is replaced. Fur, cheeks, muzzle and source alpha stay still.
    float4 surrounding=skinSample(eye.xy+float2(eye.z*1.12,0))
                      +skinSample(eye.xy-float2(eye.z*1.12,0))
                      +skinSample(eye.xy+float2(0,eye.w*1.12))
                      +skinSample(eye.xy-float2(0,eye.w*1.12));
    float3 skin=surrounding.rgb/max(.001,surrounding.a);
    float narrow=lerp(1,.18,_Blink);
    float2 compressed=eye.xy+float2(uv.x-eye.x,(uv.y-eye.y)/narrow);
    float4 pupil=tex2D(_MainTex,compressed);
    float lidAmount=smoothstep(.65,1,_Blink);
    float newInk=ink(pupil)*step(abs(eyeOffset.y),narrow)*(1-lidAmount);
    float skinLight=dot(skin,float3(.299,.587,.114));
    float originalLight=dot(original.rgb,float3(.299,.587,.114));
    float eraseInk=smoothstep(.003,.02,skinLight-originalLight);
    float3 clean=lerp(original.rgb,skin,eraseInk);
    float3 closed=lerp(clean,pupil.rgb,newInk);
    // An antialiased curved lid remains legible at the game's small portrait size.
    float curve=-.10+.32*eyeOffset.x*eyeOffset.x;
    float antialias=max(fwidth(eyeOffset.y),.015);
    float lid=(1-smoothstep(.12-antialias*.5,.12+antialias*.5,abs(eyeOffset.y-curve)))
             *(1-smoothstep(.65,.78,abs(eyeOffset.x)));
    float3 eyeColor=tex2D(_MainTex,eye.xy).rgb;
    closed=lerp(closed,eyeColor,lid*lidAmount);
    return lerp(original.rgb,closed,smoothstep(0,.08,_Blink));
   }
   float4 frag(v2f input):SV_Target {
    float4 original=tex2D(_MainTex,input.uv)+_TextureSampleAdd;
    float4 color=original;
    if (_Blink>0 && _EyeCount>0) color.rgb=closeEye(input.uv,original,_Eye0);
    if (_Blink>0 && _EyeCount>1) {
     float2 p=abs((input.uv-_Eye1.xy)/_Eye1.zw);
     if (max(p.x,p.y)<1) color.rgb=closeEye(input.uv,original,_Eye1);
    }
    color*=input.color;
    #ifdef UNITY_UI_CLIP_RECT
    color.a*=UnityGet2DClipping(input.world.xy,_ClipRect);
    #endif
    #ifdef UNITY_UI_ALPHACLIP
    clip(color.a-.001);
    #endif
    return color;
   }
   ENDCG
  }
 }
}
