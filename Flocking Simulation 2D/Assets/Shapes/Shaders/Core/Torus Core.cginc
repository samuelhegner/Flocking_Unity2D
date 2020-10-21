// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(int, _ScaleMode)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
UNITY_DEFINE_INSTANCED_PROP(float, _RadiusSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(float, _ThicknessSpace)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
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
	
	int scaleMode = UNITY_ACCESS_INSTANCED_PROP(Props, _ScaleMode);
    half uniformScale = GetUniformScale();
    half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? 1 : 1.0/uniformScale;
    
    half radiusMajorTarget = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
    int radiusMajorSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusSpace);
    int thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);
    half thicknessTarget = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness) * scaleThickness;
    
    
    // calc radius & tube center
    float3 objectOrigin = LocalToWorldPos(float3(0,0,0));
    half3 camRight = CameraToWorldVec(half3(1,0,0));
	LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( objectOrigin, camRight, radiusMajorTarget * 2 /*to thickness*/, radiusMajorSpace );
	float radiusMajor = widthDataRadius.thicknessMeters / 2;
    half3 dirFromCenter = normalize( half3( v.vertex.xy, 0 ) );
    half3 tubeCenter = dirFromCenter * radiusMajor;
    half3 tubeCenterWorld = LocalToWorldPos( tubeCenter );
	
	// calc thickness
    half3 tangentWorld = LocalToWorldVec( half3(dirFromCenter.y,-dirFromCenter.x, 0) );
    half3 camForward = GetCameraForwardDirection();
	half3 camLineNormal = normalize(cross(camForward, tangentWorld));
	LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( tubeCenterWorld, camLineNormal, thicknessTarget, thicknessSpace );
	o.pxCoverage = widthDataThickness.thicknessPixelsTarget;
	float thicknessRadius = widthDataThickness.thicknessMeters * 0.5;
	
	half3 localPos = tubeCenter + v.normal * thicknessRadius; 
	o.pos = LocalToClipPos( localPos );
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	half4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
	return ShapesOutput( color, saturate(i.pxCoverage) );
}