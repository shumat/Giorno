//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2017 //
/// Shader generate with Shadero 1.2.0                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Customs/FrozenDrop"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_SourceNewTex_1("_SourceNewTex_1(RGB)", 2D) = "white" { }
DistortionUV_WaveX_1("DistortionUV_WaveX_1", Range(0, 128)) = 36.91281
DistortionUV_WaveY_1("DistortionUV_WaveY_1", Range(0, 128)) = 11.3128
DistortionUV_DistanceX_1("DistortionUV_DistanceX_1", Range(0, 1)) = 0.546154
DistortionUV_DistanceY_1("DistortionUV_DistanceY_1", Range(0, 1)) = 0.3871802
DistortionUV_Speed_1("DistortionUV_Speed_1", Range(-2, 2)) = 0.4717955
_TintRGBA_Color_1("_TintRGBA_Color_1", COLOR) = (0.1771807,0.02205884,1,1)
_Brightness_Fade_1("_Brightness_Fade_1", Range(0, 1)) = 0
_SpriteFade("SpriteFade", Range(0, 1)) = 1.0

// required for UI.Mask
[HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
[HideInInspector]_Stencil("Stencil ID", Float) = 0
[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
[HideInInspector]_ColorMask("Color Mask", Float) = 15

}

SubShader
{

Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"}
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off

// required for UI.Mask
Stencil
{
Ref [_Stencil]
Comp [_StencilComp]
Pass [_StencilOp]
ReadMask [_StencilReadMask]
WriteMask [_StencilWriteMask]
}

Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t{
float4 vertex   : POSITION;
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
float2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
float4 color    : COLOR;
};

sampler2D _MainTex;
float _SpriteFade;
sampler2D _SourceNewTex_1;
float DistortionUV_WaveX_1;
float DistortionUV_WaveY_1;
float DistortionUV_DistanceX_1;
float DistortionUV_DistanceY_1;
float DistortionUV_Speed_1;
float4 _TintRGBA_Color_1;
float _Brightness_Fade_1;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}


float2 DistortionUV(float2 p, float WaveX, float WaveY, float DistanceX, float DistanceY, float Speed)
{
Speed *=_Time*100;
p.x= p.x+sin(p.y*WaveX + Speed)*DistanceX*0.05;
p.y= p.y+cos(p.x*WaveY + Speed)*DistanceY*0.05;
return p;
}
float4 UniColor(float4 txt, float4 color)
{
txt.rgb = lerp(txt.rgb,color.rgb,color.a);
return txt;
}
float4 TintRGBA(float4 txt, float4 color)
{
float3 tint = dot(txt.rgb, float3(.222, .707, .071));
tint.rgb *= color.rgb;
txt.rgb = lerp(txt.rgb,tint.rgb,color.a);
return txt;
}
float4 Brightness(float4 txt, float value)
{
txt.rgb += value;
return txt;
}
float2 AnimatedTwistUV(float2 uv, float value, float posx, float posy, float radius, float speed)
{
float2 center = float2(posx, posy);
float2 tc = uv - center;
float dist = length(tc);
if (dist < radius)
{
float percent = (radius - dist) / radius;
float theta = percent * percent * 16.0 * sin(value);
float s = sin(theta + _Time.y * speed);
float c = cos(theta + _Time.y * speed);
tc = float2(dot(tc, float2(c, -s)), dot(tc, float2(s, c)));
}
tc += center;
return tc;
}
float DSFXr (float2 c, float seed)
{
return frac(43.*sin(c.x+7.*c.y)*seed);
}

float DSFXn (float2 p, float seed)
{
float2 i = floor(p), w = p-i, j = float2 (1.,0.);
w = w*w*(3.-w-w);
return lerp(lerp(DSFXr(i, seed), DSFXr(i+j, seed), w.x), lerp(DSFXr(i+j.yx, seed), DSFXr(i+1., seed), w.x), w.y);
}

float DSFXa (float2 p, float seed)
{
float m = 0., f = 2.;
for ( int i=0; i<9; i++ ){ m += DSFXn(f*p, seed)/f; f+=f; }
return m;
}

float4 DestroyerFX(float4 txt, float2 uv, float value, float seed, float HDR)
{
float t = frac(value*0.9999);
float4 c = smoothstep(t / 1.2, t + .1, DSFXa(3.5*uv, seed));
c = txt*c;
c.r = lerp(c.r, c.r*120.0*(1 - c.a), value);
c.g = lerp(c.g, c.g*40.0*(1 - c.a), value);
c.b = lerp(c.b, c.b*5.0*(1 - c.a) , value);
c.rgb = lerp(saturate(c.rgb),c.rgb,HDR);
return c;
}
float4 Generate_Circle(float2 uv, float posX, float posY, float Size, float Smooth, float black)
{
float2 center = float2(posX, posY);
float dist = 1.0 - smoothstep(Size, Size + Smooth, length(center - uv));
float4 result = float4(1,1,1,dist);
if (black == 1) result = float4(dist, dist, dist, 1);
return result;
}
float4 OperationBlend(float4 origin, float4 overlay, float blend)
{
float4 o = origin; 
o.a = overlay.a + origin.a * (1 - overlay.a);
o.rgb = (overlay.rgb * overlay.a + origin.rgb * origin.a * (1 - overlay.a)) / (o.a+0.0000001);
o = lerp(origin, o, blend);
return saturate(o);
}
float4 Color_PreGradients(float4 rgba, float4 a, float4 b, float4 c, float4 d, float offset, float fade, float speed)
{
float gray = (rgba.r + rgba.g + rgba.b) / 3;
gray += offset+(speed*_Time*20);
float4 result = a + b * cos(6.28318 * (c * gray + d));
result.a = rgba.a;
result.rgb = lerp(rgba.rgb, result.rgb, fade);
return result;
}
float2 KaleidoscopeUV(float2 uv, float posx, float posy, float number)
{
uv = uv - float2(posx, posy);
float r = length(uv);
float a = abs(atan2(uv.y, uv.x));
float sides = number;
float tau = 3.1416;
a = fmod(a, tau / sides);
a = abs(a - tau / sides / 2.);
uv = r * float2(cos(a), sin(a));
return uv;
}
float4 frag (v2f i) : COLOR
{
float4 _Generate_Circle_2 = Generate_Circle(i.texcoord,0.5,0.5,0.3820535,0.05257867,0);
float2 AnimatedTwistUV_1 = AnimatedTwistUV(i.texcoord,0.2307692,0.5,0.5,0.4269228,0.05128322);
float2 KaleidoscopeUV_1 = KaleidoscopeUV(AnimatedTwistUV_1,0.5,0.5,3.00763);
float2 DistortionUV_1 = DistortionUV(KaleidoscopeUV_1,DistortionUV_WaveX_1,DistortionUV_WaveY_1,DistortionUV_DistanceX_1,DistortionUV_DistanceY_1,DistortionUV_Speed_1);
float4 SourceRGBA_1 = tex2D(_SourceNewTex_1, DistortionUV_1);
float4 _PremadeGradients_1 = Color_PreGradients(SourceRGBA_1,float4(0.5,0.5,0.5,1),float4(0.5,0.5,0.5,1),float4(0.9,0.9,0.9,1),float4(0.47,0.57,0.67,1),0,1,0);
float4 _Generate_Circle_3 = Generate_Circle(i.texcoord,0.5,0.5,-0.08715661,0.5894873,0);
_PremadeGradients_1 = lerp(_PremadeGradients_1,_PremadeGradients_1*_PremadeGradients_1.a + _Generate_Circle_3*_Generate_Circle_3.a,1);
float4 _Generate_Circle_1 = Generate_Circle(i.texcoord,0.5,0.5,0.4,0.01,0);
float4 TintRGBA_1 = TintRGBA(_Generate_Circle_1,_TintRGBA_Color_1);
_PremadeGradients_1 = lerp(_PremadeGradients_1,_PremadeGradients_1 * TintRGBA_1,1);
float4 OperationBlend_1 = OperationBlend(_Generate_Circle_2, _PremadeGradients_1, 1); 
float4 Brightness_1 = Brightness(OperationBlend_1,_Brightness_Fade_1);
float4 FinalResult = Brightness_1;
FinalResult.rgb *= i.color.rgb;
FinalResult.a = FinalResult.a * _SpriteFade * i.color.a;
return FinalResult;
}

ENDCG
}
}
Fallback "Sprites/Default"
}
