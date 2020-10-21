// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(int, _ScaleMode)
UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_DEFINE_INSTANCED_PROP(float4, _ColorEnd)
UNITY_DEFINE_INSTANCED_PROP(float3, _PointStart)
UNITY_DEFINE_INSTANCED_PROP(float3, _PointEnd)
UNITY_DEFINE_INSTANCED_PROP(float, _Thickness)
UNITY_DEFINE_INSTANCED_PROP(int, _ThicknessSpace)
UNITY_DEFINE_INSTANCED_PROP(float, _DashSize)
UNITY_DEFINE_INSTANCED_PROP(float, _DashOffset)
UNITY_DEFINE_INSTANCED_PROP(float, _DashSpacing)
UNITY_DEFINE_INSTANCED_PROP(int, _DashSpace)
UNITY_DEFINE_INSTANCED_PROP(int, _DashSnap)
UNITY_INSTANCING_BUFFER_END(Props)

#define IP_dash_coord intp0.x
#define IP_dash_spacePerPeriod intp0.y
#define IP_dash_thicknessPerPeriod intp0.z
#define IP_pxCoverage intp0.w

struct VertexInput {
	float4 vertex : POSITION;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
	float4 pos : SV_POSITION;
	half4 intp0 : TEXCOORD1;
	#if defined(CAP_SQUARE)
		half colorBlend : TEXCOORD0; // needed since we need unclamped color blend value in the frag shader
	#else
		half4 color : TEXCOORD0;
	#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert(VertexInput v) {
	UNITY_SETUP_INSTANCE_ID(v);
	VertexOutput o = (VertexOutput)0;
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	float3 a = LocalToWorldPos( UNITY_ACCESS_INSTANCED_PROP(Props, _PointStart) );
	float3 b = LocalToWorldPos( UNITY_ACCESS_INSTANCED_PROP(Props, _PointEnd) );
	
	int scaleMode = UNITY_ACCESS_INSTANCED_PROP(Props, _ScaleMode);
    half uniformScale = GetUniformScale();
	half scaleThickness = scaleMode == SCALE_MODE_UNIFORM ? uniformScale : 1;
	half scaleDashes = uniformScale;
	half scaleSpacing = uniformScale;
	 
	half thickness = UNITY_ACCESS_INSTANCED_PROP(Props, _Thickness) * scaleThickness;
	int thicknessSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _ThicknessSpace);

	half lineLength;
	half3 right;
	half3 normal;
	half3 forward;
	GetDirMag(b - a, /*out*/ forward, /*out*/ lineLength);

    if( lineLength < 0.0001 ){ // degenerate case (start == end)
        right   = half3(1,0,0);
        normal  = half3(0,1,0);
        forward = half3(0,0,1);
    } else {
        bool prettyVertical = abs(forward.y) >= 0.99;
        half3 upRef = prettyVertical ? half3(1,0,0) : half3(0,1,0);
        normal = normalize(cross(upRef,forward));
        right = cross( normal, forward );
    }

	float side = saturate( v.uv0.y );
	float3 vertOrigin = side > 0.5 ? b : a;
	half3 camForward = DirectionToNearPlanePos(vertOrigin);
	half3 camLineNormal = normalize(cross(camForward, forward));
    LineWidthData widthData = GetScreenSpaceWidthDataSimple( vertOrigin, camLineNormal, thickness, thicknessSpace );
    o.IP_pxCoverage = widthData.thicknessPixelsTarget;
    float radius = widthData.thicknessMeters * 0.5;
	
	half3 localOffset = v.vertex - half3( 0, 0, saturate( v.uv0.y ) ); //  if uv >= 1 then subtract height (z) by 1 to make it a spherical offset
	localOffset *= radius;
	half3 vertPos = vertOrigin + localOffset.x * right + localOffset.y * normal;
	
	#ifdef CAP_ROUND
	    vertPos += localOffset.z * forward;
	#elif defined(CAP_SQUARE)
	    vertPos += (side*2-1) * forward * radius;
	#endif
	
	#if defined(CAP_SQUARE)
	    half k = 2 * radius / lineLength + 1;
        half m = -radius / lineLength;
        o.colorBlend = k * side + m;
	#else
        half4 colorStart = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
        half4 colorEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd); // todo: make the gradient thing be a thing
	    o.color = lerp( colorStart, colorEnd, side );
	#endif
	
	// dashes
	half dashSizeInput = UNITY_ACCESS_INSTANCED_PROP(Props, _DashSize);
	if( dashSizeInput > 0 ){
		float dashOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _DashOffset);
		int dashSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _DashSpace);
		half size = dashSizeInput * scaleDashes;
		half spacing = UNITY_ACCESS_INSTANCED_PROP(Props, _DashSpacing) * scaleSpacing;
		bool snap = UNITY_ACCESS_INSTANCED_PROP(Props, _DashSnap) > 0;
		half projDist = dot( forward, vertPos - a ); // distance along line
		LineDashData dashData = GetDashCoordinates( size, spacing, projDist, lineLength, widthData.thicknessMeters, thicknessSpace, widthData.pxPerMeter, dashOffset, dashSpace, snap );
		o.IP_dash_coord = dashData.coord;
		o.IP_dash_spacePerPeriod = dashData.spacePerPeriod;
		o.IP_dash_thicknessPerPeriod = dashData.thicknessPerPeriod;
	}

	o.pos = WorldToClipPos( vertPos.xyz );
	return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
	UNITY_SETUP_INSTANCE_ID(i);
	 
    #if defined(CAP_SQUARE)
        // interpolation of colors is done here because we need to clamp the color blend value in the frag shader
        // due to that being calculated in the vert shader, but the 0 and 1 crossings are offset from the vert
        // todo: use a proper cylinder mesh with extra verts for 0 and 1 crossings
        half4 colorStart = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
        half4 colorEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd); // todo: make the gradient thing be a thing
	    half4 shape_color = lerp( colorStart, colorEnd, saturate(i.colorBlend) );
    #else
	    half4 shape_color = i.color;
    #endif
    
    half shape_mask = 1;
	LineDashData dashData;
	dashData.coord = i.IP_dash_coord;
	dashData.spacePerPeriod = i.IP_dash_spacePerPeriod;
	dashData.thicknessPerPeriod = i.IP_dash_thicknessPerPeriod;
	ApplyDashMask( /*inout*/ shape_mask, dashData, 0, 0 );
	    
    shape_mask *= saturate(i.IP_pxCoverage);
	return ShapesOutput( shape_color, shape_mask );
}