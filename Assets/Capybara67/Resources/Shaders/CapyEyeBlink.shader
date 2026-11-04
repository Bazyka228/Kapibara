Shader "Capybara67/UI Eyes Only" {
 Properties {
  [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
  _Color ("Tint", Color) = (1,1,1,1)
  _Blink ("Eye closure", Range(0,1)) = 0
  _Wink ("Near eye expression", Range(0,1)) = 0
  _EyeCount ("Open eyes", Float) = 0
  _Eye0 ("First eye UV", Vector) = (0,0,0,0)
  _Eye1 ("Second eye UV", Vector) = (0,0,0,0)
  _Skin ("Fur design", Float) = 0
  _SkinTime ("Fur shimmer time", Float) = 0
  _PortraitUV ("Portrait UV", Vector) = (0,0,1,1)
  _FurMask ("Visible fur selection", 2D) = "white" {}
  _UseFurMask ("Protect outfit fabric", Float) = 0
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
   sampler2D _MainTex, _FurMask;
   float4 _Color, _TextureSampleAdd, _ClipRect, _Eye0, _Eye1;
   float _Blink, _Wink, _EyeCount, _Skin, _SkinTime, _UseFurMask;
   float4 _PortraitUV;
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
   float3 closeEye(float2 uv,float4 original,float4 eye,float closure) {
    float2 eyeOffset=(uv-eye.xy)/eye.zw;
    if (max(abs(eyeOffset.x),abs(eyeOffset.y))>=1) return original.rgb;
    // Only the dark eye ink is replaced. Fur, cheeks, muzzle and source alpha stay still.
    float4 surrounding=skinSample(eye.xy+float2(eye.z*1.12,0))
                      +skinSample(eye.xy-float2(eye.z*1.12,0))
                      +skinSample(eye.xy+float2(0,eye.w*1.12))
                      +skinSample(eye.xy-float2(0,eye.w*1.12));
    float3 skin=surrounding.rgb/max(.001,surrounding.a);
    float narrow=lerp(1,.18,closure);
    float2 compressed=eye.xy+float2(uv.x-eye.x,(uv.y-eye.y)/narrow);
    float4 pupil=tex2D(_MainTex,compressed);
    float lidAmount=smoothstep(.65,1,closure);
    float newInk=ink(pupil)*step(abs(eyeOffset.y),narrow)*(1-lidAmount);
    float skinLight=dot(skin,float3(.299,.587,.114));
    float originalLight=dot(original.rgb,float3(.299,.587,.114));
    float eraseInk=smoothstep(.003,.02,skinLight-originalLight);
    float3 clean=lerp(original.rgb,skin,eraseInk);
    float3 closed=lerp(clean,pupil.rgb,newInk);
    // An antialiased curved lid remains legible at the game's small portrait size.
    float curve=-.10+.32*eyeOffset.x*eyeOffset.x;
    float antialias=max(fwidth(eyeOffset.y),.015);
    float lidWidth=max(.12,antialias*.70);
    float lid=(1-smoothstep(lidWidth-antialias*.5,lidWidth+antialias*.5,abs(eyeOffset.y-curve)))
             *(1-smoothstep(.65,.78,abs(eyeOffset.x)));
    float3 eyeColor=tex2D(_MainTex,eye.xy).rgb;
    closed=lerp(closed,eyeColor,lid*lidAmount);
    return lerp(original.rgb,closed,smoothstep(0,.08,closure));
   }
   float hash21(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float3 furDesign(float3 c,float2 uv){
    if(_Skin<.5)return c;
    float3 source=c;
    #ifndef UNITY_COLORSPACE_GAMMA
    c=LinearToGammaSpace(c);
    #endif
    float2 p=(uv-_PortraitUV.xy)/max(_PortraitUV.zw-_PortraitUV.xy,.00001);
    // Recolour caramel fur only: retain dark outlines, eyes, muzzle shading and pink cheeks.
    float mask=smoothstep(.67,.79,c.r)*smoothstep(.46,.58,c.g)*(1-smoothstep(.32,.44,c.r-c.g));
    if(_UseFurMask>.5)mask*=tex2D(_FurMask,uv).r;
    float light=dot(c,float3(.299,.587,.114));
    float3 baseColor=float3(.75,.42,.25),accent=float3(.98,.78,.48);
    float detail=0,shine=0;
    float2 cell=frac(p*float2(18,22))-.5;
    if(_Skin<1.5){baseColor=float3(.72,.38,.23);accent=float3(.96,.73,.48);detail=smoothstep(.72,.96,sin(p.y*52+p.x*13))*.5;}
    else if(_Skin<2.5){baseColor=float3(.63,.58,.69);accent=float3(.9,.85,.96);detail=(1-smoothstep(.10,.18,length(cell)))*.5;}
    else if(_Skin<3.5){baseColor=lerp(float3(.39,.27,.61),float3(.34,.71,.67),p.y);accent=float3(.68,.95,.84);detail=pow(.5+.5*sin(p.x*38+p.y*26),12)*.6;shine=.08*sin(p.y*12-_SkinTime*.7);}
    else if(_Skin<4.5){baseColor=float3(.77,.59,.34);accent=float3(.98,.86,.56);detail=step(.65,hash21(floor(p*float2(17,20))))*.46;}
    else if(_Skin<5.5){baseColor=float3(.96,.68,.28);accent=float3(1,.92,.65);float a=atan2(p.y-.55,p.x-.52);detail=pow(.5+.5*cos(a*12),8)*.32;shine=.05*sin(p.y*9-_SkinTime*.5);}
    else if(_Skin<6.5){baseColor=lerp(float3(.3,.31,.52),float3(.57,.52,.76),p.y);accent=float3(.88,.92,1);float star=(1-smoothstep(.025,.09,abs(cell.x)*abs(cell.y)))*(1-smoothstep(.13,.27,length(cell)));detail=star*step(.72,hash21(floor(p*float2(18,22))))*.95;shine=.04*sin(p.y*12+p.x*8-_SkinTime*.6);}
    else if(_Skin<7.5){baseColor=float3(.94,.85,.7);accent=float3(1,.97,.87);detail=pow(.5+.5*sin(p.y*180+sin(p.x*125)*2),6)*.13;}
    else if(_Skin<8.5){baseColor=float3(.53,.69,.71);accent=float3(.82,.98,.83);float2 circuit=frac(p*float2(9,11));detail=(1-smoothstep(.025,.05,min(circuit.x,circuit.y)))*.52;shine=.06*pow(.5+.5*sin(p.y*8-_SkinTime*.7),8);}
    else if(_Skin<9.5){baseColor=float3(.89,.64,.24);accent=float3(1,.94,.65);detail=pow(.5+.5*sin(p.y*38+p.x*8),18)*.15;shine=.13*pow(.5+.5*sin(p.x*5+p.y*2-_SkinTime*.4),10);}
    else {baseColor=float3(.94,.65,.43);accent=float3(1,.83,.62);detail=pow(.5+.5*sin(p.y*135+sin(p.x*100)),8)*.08;}
    float3 painted=lerp(baseColor,accent,detail)*(.65+light*.55)+shine;
    #ifndef UNITY_COLORSPACE_GAMMA
    painted=GammaToLinearSpace(painted);
    #endif
    return lerp(source,painted,saturate(mask)*.96);
   }
   float4 frag(v2f input):SV_Target {
    float4 original=tex2D(_MainTex,input.uv)+_TextureSampleAdd;
    float4 color=original;
    if (max(_Blink,_Wink)>0 && _EyeCount>0) color.rgb=closeEye(input.uv,original,_Eye0,max(_Blink,_Wink));
    if (_Blink>0 && _EyeCount>1) {
     float2 p=abs((input.uv-_Eye1.xy)/_Eye1.zw);
     if (max(p.x,p.y)<1) color.rgb=closeEye(input.uv,original,_Eye1,_Blink);
    }
    color.rgb=furDesign(color.rgb,input.uv);
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
