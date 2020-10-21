// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(int, _ScaleMode)
UNITY_DEFINE_INSTANCED_PROP(int, _Alignment)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float, _Radius)
UNITY_DEFINE_INSTANCED_PROP(int, _RadiusSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(int, _ThicknessSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _Angle)
UNITY_DEFINE_INSTANCED_PROP(float, _Roundness)
UNITY_DEFINE_INSTANCED_PROP(int, _Hollow)
UNITY_DEFINE_INSTANCED_PROP(int, _Sides)
SHAPES_FILL_PROPERTIES
UNITY_INSTANCING_BUFFER_END(Props)

#define ALIGNMENT_FLAT 0
#define ALIGNMENT_BILLBOARD 1

#define IP_uv0 intp0.xy
#define IP_pxPerMeter intp0.z
#define IP_apothem intp0.w
#define IP_innerRadiusFraction intp1.x
#define IP_pxCoverage intp1.y
#define IP_angHalfSegment intp1.z
#define IP_halfSideLength intp1.w

struct VertexInput {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    float4 pos : SV_POSITION;
    half4 intp0 : TEXCOORD0;
	half4 intp1 : TEXCOORD1;
	SHAPES_INTERPOLATOR_FILL(2)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert (VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Radius);
	float radiusSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusSpace);
	float3 wPos = LocalToWorldPos( float3(0,0,0) ); // per vertex makes it real wonky so shrug~
    float3 camRight = CameraToWorldVec( float3(1,0,0) );
    LineWidthData widthDataRadius = GetScreenSpaceWidthDataSimple( wPos, camRight, radius*2, radiusSpace );
    o.IP_pxCoverage = widthDataRadius.thicknessPixelsTarget;
    half radiusInMeters = widthDataRadius.thicknessMeters / 2; // actually, center radius

	bool hollow = UNITY_ACCESS_INSTANCED_PROP(Props, _Hollow) == 1;
	if( hollow ) {
		o.IP_pxPerMeter = widthDataRadius.pxPerMeter;
		int scaleMode = UNITY_ACCESS_INSTANCED_PROP(Props, _ScaleMode);
		half uniformScale = GetUniformScale();
		half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? 1 : 1.0/uniformScale;
		float thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness) * scaleThickness;
		float thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);
		LineWidthData widthDataThickness = GetScreenSpaceWidthDataSimple( wPos, camRight, thickness, thicknessSpace );
		half thicknessRadius = widthDataThickness.thicknessMeters / 2;
		o.IP_pxCoverage = widthDataThickness.thicknessPixelsTarget; // todo: this isn't properly handling coordinate scaling yet
		half radiusOuter = radiusInMeters + thicknessRadius;
		o.IP_innerRadiusFraction = (radiusOuter - thicknessRadius*2) / radiusOuter;
		v.vertex.xy = v.uv0 * radiusOuter;
	} else {
		v.vertex.xy = v.uv0 * radiusInMeters;
	}

	if( UNITY_ACCESS_INSTANCED_PROP(Props, _Alignment) == ALIGNMENT_BILLBOARD ) {
		half3 frw = WorldToLocalVec( -DirectionToNearPlanePos( wPos ) );
		half3 camRightLocal = WorldToLocalVec( camRight );
		half3 up = normalize( cross( frw, camRightLocal ) );
		half3 right = cross( up, frw ); // already normalized
		v.vertex.xyz = v.vertex.x * right + v.vertex.y * up;
	}

	float n = UNITY_ACCESS_INSTANCED_PROP(Props, _Sides);
	half angSegment = TAU/n;
	o.IP_angHalfSegment = angSegment/2;
	o.IP_apothem = cos( o.IP_angHalfSegment );
	o.IP_halfSideLength = sin( o.IP_angHalfSegment );

	SHAPES_TRANSFER_FILL
	
	o.IP_uv0 = v.uv0;
    o.pos = UnityObjectToClipPos( v.vertex );

    return o;
}

inline half GetRadialMask( VertexOutput i ){
	float angOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _Angle);
	float n = UNITY_ACCESS_INSTANCED_PROP(Props, _Sides);
	half angSegment = TAU/n;

	half2 coords = i.IP_uv0;
	half roundness = UNITY_ACCESS_INSTANCED_PROP(Props, _Roundness);
	half roundnessInv = 1.0-roundness;
	half sdf = SdfNgon( angSegment, i.IP_apothem*roundnessInv, i.IP_halfSideLength*roundnessInv, angOffset, coords );
	half mask = StepAA( sdf, roundness*i.IP_apothem ); // outer radius
	bool hollow = UNITY_ACCESS_INSTANCED_PROP(Props, _Hollow) == 1;
	if( hollow )
		mask = min( mask, 1.0-StepAA( sdf + 1-i.IP_innerRadiusFraction, roundness*i.IP_apothem ) ); // inner radius
	return mask;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	half mask = GetRadialMask( i );
	mask *= saturate(i.IP_pxCoverage); // pixel fade
	half4 fillColor = SHAPES_GET_FILL_COLOR
	return ShapesOutput( fillColor, mask );
}


