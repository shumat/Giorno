//////////////////////////////////////////////////////////////
/// Shadero Sprite: Sprite Shader Editor - by VETASOFT 2017 //
/// Shader generate with Shadero 1.2.0                      //
/// http://u3d.as/V7t #AssetStore                           //
/// http://www.shadero.com #Docs                            //
//////////////////////////////////////////////////////////////

Shader "Shadero Customs/Material_01"
{
Properties
{
[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
_SourceNewTex_1("_SourceNewTex_1(RGB)", 2D) = "white" { }
RotationUV_Rotation_1("RotationUV_Rotation_1", Range(-360, 360)) = 24.92291
RotationUV_Rotation_PosX_1("RotationUV_Rotation_PosX_1", Range(-1, 2)) = 0.5
RotationUV_Rotation_PosY_1("RotationUV_Rotation_PosY_1", Range(-1, 2)) =0.5
KaleidoscopeUV_PosX_1("KaleidoscopeUV_PosX_1",  Range(-2, 2)) = 0.5
KaleidoscopeUV_PosY_1("KaleidoscopeUV_PosY_1",  Range(-2, 2)) = 0.5
KaleidoscopeUV_Number_1("KaleidoscopeUV_Number_1", Range(0, 6)) = 3.976823
ZoomUV_Zoom_1("ZoomUV_Zoom_1", Range(0.2, 4)) = 0.7436538
ZoomUV_PosX_1("ZoomUV_PosX_1", Range(-3, 3)) = -0.03846154
ZoomUV_PosY_1("ZoomUV_PosY_1", Range(-3, 3)) =0.2692308
DistortionUV_WaveX_1("DistortionUV_WaveX_1", Range(0, 128)) = 0
DistortionUV_WaveY_1("DistortionUV_WaveY_1", Range(0, 128)) = 30.51282
DistortionUV_DistanceX_1("DistortionUV_DistanceX_1", Range(0, 1)) = 0
DistortionUV_DistanceY_1("DistortionUV_DistanceY_1", Range(0, 1)) = 0.5179487
DistortionUV_Speed_1("DistortionUV_Speed_1", Range(-2, 2)) = 0.948718
_Generate_Circle_PosX_1("_Generate_Circle_PosX_1", Range(-1, 2)) = 0.5
_Generate_Circle_PosY_1("_Generate_Circle_PosY_1", Range(-1, 2)) = 0.5
_Generate_Circle_Size_1("_Generate_Circle_Size_1", Range(-1, 1)) = 0.06922717
_Generate_Circle_Dist_1("_Generate_Circle_Dist_1", Range(0, 1)) = 0.1294857
_RGBA_Mul_Fade_1("_RGBA_Mul_Fade_1", Range(0, 2)) = 0.9641019
_MaskChannel_Fade_1("_MaskChannel_Fade_1", Range(0, 1)) = 1
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
float RotationUV_Rotation_1;
float RotationUV_Rotation_PosX_1;
float RotationUV_Rotation_PosY_1;
float KaleidoscopeUV_PosX_1;
float KaleidoscopeUV_PosY_1;
float KaleidoscopeUV_Number_1;
float ZoomUV_Zoom_1;
float ZoomUV_PosX_1;
float ZoomUV_PosY_1;
float DistortionUV_WaveX_1;
float DistortionUV_WaveY_1;
float DistortionUV_DistanceX_1;
float DistortionUV_DistanceY_1;
float DistortionUV_Speed_1;
float _Generate_Circle_PosX_1;
float _Generate_Circle_PosY_1;
float _Generate_Circle_Size_1;
float _Generate_Circle_Dist_1;
float _RGBA_Mul_Fade_1;
float _MaskChannel_Fade_1;
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
float2 RotationUV(float2 uv, float rot, float posx, float posy)
{
uv = uv - float2(posx, posy);
float angle = rot * 0.01744444;
float sinX = sin(angle);
float cosX = cos(angle);
float2x2 rotationMatrix = float2x2(cosX, -sinX, sinX, cosX);
uv = mul(uv, rotationMatrix) + float2(posx, posy);
return uv;
}
float4 TintRGBA(float4 txt, float4 color)
{
float3 tint = dot(txt.rgb, float3(.222, .707, .071));
tint.rgb *= color.rgb;
txt.rgb = lerp(txt.rgb,tint.rgb,color.a);
return txt;
}
float4 InverseColor(float4 txt, float fade)
{
float3 gs = 1 - txt.rgb;
return lerp(txt, float4(gs, txt.a), fade);
}
float4 Brightness(float4 txt, float value)
{
txt.rgb += value;
return txt;
}
float2 ZoomUV(float2 uv, float zoom, float posx, float posy)
{
float2 center = float2(posx, posy);
uv -= center;
uv = uv * zoom;
uv += center;
return uv;
}
float4 Generate_Circle(float2 uv, float posX, float posY, float Size, float Smooth, float black)
{
float2 center = float2(posX, posY);
float dist = 1.0 - smoothstep(Size, Size + Smooth, length(center - uv));
float4 result = float4(1,1,1,dist);
if (black == 1) result = float4(dist, dist, dist, 1);
return result;
}
float4 Generate_Shape(float2 uv, float posX, float posY, float Size, float Smooth, float number, float black, float rot)
{
uv = uv - float2(posX, posY);
float angle = rot * 0.01744444;
float a = atan2(uv.x, uv.y) +angle, r = 6.28318530718 / int(number);
float d = cos(floor(0.5 + a / r) * r - a) * length(uv);
float dist = 1.0 - smoothstep(Size, Size + Smooth, d);
float4 result = float4(1, 1, 1, dist);
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
float Generate_Fire_hash2D(float2 x)
{
return frac(sin(dot(x, float2(13.454, 7.405)))*12.3043);
}

float Generate_Fire_voronoi2D(float2 uv, float precision)
{
float2 fl = floor(uv);
float2 fr = frac(uv);
float res = 1.0;
for (int j = -1; j <= 1; j++)
{
for (int i = -1; i <= 1; i++)
{
float2 p = float2(i, j);
float h = Generate_Fire_hash2D(fl + p);
float2 vp = p - fr + h;
float d = dot(vp, vp);
res += 1.0 / pow(d, 8.0);
}
}
return pow(1.0 / res, precision);
}

float4 Generate_Fire(float2 uv, float posX, float posY, float precision, float smooth, float speed, float black)
{
uv += float2(posX, posY);
float t = _Time*60*speed;
float up0 = Generate_Fire_voronoi2D(uv * float2(6.0, 4.0) + float2(0, -t), precision);
float up1 = 0.5 + Generate_Fire_voronoi2D(uv * float2(6.0, 4.0) + float2(42, -t ) + 30.0, precision);
float finalMask = up0 * up1  + (1.0 - uv.y);
finalMask += (1.0 - uv.y)* 0.5;
finalMask *= 0.7 - abs(uv.x - 0.5);
float4 result = smoothstep(smooth, 0.95, finalMask);
result.a = saturate(result.a + black);
return result;
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
float4 FadeToAlpha(float4 txt,float fade)
{
return float4(txt.rgb, txt.a*fade);
}

float4 frag (v2f i) : COLOR
{
float4 _Generate_Shape_2 = Generate_Shape(i.texcoord,0.5,0.5,0.3897438,0.04846193,10,0,0);
float4 _Generate_Shape_3 = Generate_Shape(i.texcoord,0.5,0.5,0.1743598,0.3179497,10,1,0);
float4 InverseColor_2 = InverseColor(_Generate_Shape_3,1);
float4 RGBASplit_1 = InverseColor_2;
InverseColor_2.a = RGBASplit_1.r;
float4 FadeToAlpha_1 = FadeToAlpha(InverseColor_2,1);
float2 RotationUV_1 = RotationUV(i.texcoord,RotationUV_Rotation_1,RotationUV_Rotation_PosX_1,RotationUV_Rotation_PosY_1);
float2 KaleidoscopeUV_1 = KaleidoscopeUV(RotationUV_1,KaleidoscopeUV_PosX_1,KaleidoscopeUV_PosY_1,KaleidoscopeUV_Number_1);
float2 ZoomUV_1 = ZoomUV(KaleidoscopeUV_1,ZoomUV_Zoom_1,ZoomUV_PosX_1,ZoomUV_PosY_1);
float2 DistortionUV_1 = DistortionUV(ZoomUV_1,DistortionUV_WaveX_1,DistortionUV_WaveY_1,DistortionUV_DistanceX_1,DistortionUV_DistanceY_1,DistortionUV_Speed_1);
float4 SourceRGBA_1 = tex2D(_SourceNewTex_1, DistortionUV_1);
float4 _Generate_Circle_1 = Generate_Circle(i.texcoord,_Generate_Circle_PosX_1,_Generate_Circle_PosY_1,_Generate_Circle_Size_1,_Generate_Circle_Dist_1,1);
_Generate_Circle_1.r *= _RGBA_Mul_Fade_1;
float4 RGBASplit_2 = _Generate_Circle_1;
SourceRGBA_1.a = lerp(RGBASplit_2.r * SourceRGBA_1.a, (1 - RGBASplit_2.r) * SourceRGBA_1.a,_MaskChannel_Fade_1);
float4 InverseColor_1 = InverseColor(SourceRGBA_1,1);
float4 _PremadeGradients_1 = Color_PreGradients(InverseColor_1,float4(0.55,0.55,0.55,1),float4(0.8,0.8,0.8,1),float4(0.29,0.29,0.29,1),float4(0.54,0.59,0.6900001,1),0.4794858,1,0);
FadeToAlpha_1 = lerp(FadeToAlpha_1,FadeToAlpha_1*FadeToAlpha_1.a + _PremadeGradients_1*_PremadeGradients_1.a,1);
float4 TintRGBA_2 = TintRGBA(FadeToAlpha_1, float4(0.7867647,0.9470588,1,1));
float4 _Generate_Shape_1 = Generate_Shape(i.texcoord,0.5,0.5,0.4076906,0,10,0,0);
float4 TintRGBA_1 = TintRGBA(_Generate_Shape_1, float4(0.9338235,0.98357,1,1));
TintRGBA_2 = lerp(TintRGBA_2,TintRGBA_2 * TintRGBA_1,1);
float4 OperationBlend_1 = OperationBlend(_Generate_Shape_2, TintRGBA_2, 1); 
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
