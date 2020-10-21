// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float3, _Size)
UNITY_DEFINE_INSTANCED_PROP(int, _SizeSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	half pxCoverage : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
    //float radiusTarget = 1;//UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
    float3 size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
    int radiusSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _SizeSpace);
    
    float3 center = LocalToWorldPos( float3( 0, 0, 0 ) );
    float3 camRight = CameraToWorldVec( float3( 1, 0, 0 ) );
	LineWidthData widthData = GetScreenSpaceWidthDataSimple( center, camRight, 1, radiusSpace );
    half radius = widthData.thicknessMeters * 0.5;
    o.pxCoverage = widthData.thicknessPixelsTarget;
    
	half3 localPos = v.vertex.xyz * size.xyz * radius;
	o.pos = LocalToClipPos( localPos );
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	half4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
	return ShapesOutput( color, saturate( i.pxCoverage ) ); 
}