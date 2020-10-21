// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "Shapes Config.cginc"
#include "Shapes Math.cginc"

// macros
#define SHAPES_FILL_PROPERTIES \
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorEnd) \
UNITY_DEFINE_INSTANCED_PROP( int, _FillType) \
UNITY_DEFINE_INSTANCED_PROP( int, _FillSpace) \
UNITY_DEFINE_INSTANCED_PROP( float4, _FillStart) /* xyz = pos, w = radius*/ \
UNITY_DEFINE_INSTANCED_PROP( float3, _FillEnd) /* xyz = pos */

#define SHAPES_INTERPOLATOR_FILL(i) float3 fillCoords : TEXCOORD##i;

#define SHAPES_TRANSFER_FILL \
int fillType = UNITY_ACCESS_INSTANCED_PROP(Props, _FillType ); \
int fillSpace = UNITY_ACCESS_INSTANCED_PROP(Props, _FillSpace ); \
float4 fillStart = UNITY_ACCESS_INSTANCED_PROP(Props, _FillStart ); \
float3 fillEnd = UNITY_ACCESS_INSTANCED_PROP(Props, _FillEnd ); \
o.fillCoords = GetFillCoords( v.vertex.xyz, fillType, fillSpace, fillStart, fillEnd );

#define SHAPES_GET_FILL_COLOR GetFillColor( i.fillCoords, \
UNITY_ACCESS_INSTANCED_PROP(Props, _FillType ), \
UNITY_ACCESS_INSTANCED_PROP(Props, _FillStart ), \
UNITY_ACCESS_INSTANCED_PROP(Props, _Color ), \
UNITY_ACCESS_INSTANCED_PROP(Props, _ColorEnd ) \
);


float3 GetFillCoords( float3 localPos, int fillType, int fillSpace, float4 start, float3 end ){
    if( fillType != FILL_TYPE_NONE ){
        // need coords
        float3 absoluteCoord = fillSpace == FILL_SPACE_LOCAL ? localPos : LocalToWorldPos( localPos );
        float3 relativeCoord = absoluteCoord - start.xyz;
        
        if( fillType == FILL_TYPE_RADIAL ){
            // has to send full coordinates
            return relativeCoord; 
        } else {
            // linear needs only the interpolator
            half3 gradVec = end - start.xyz;
            half t = dot(gradVec, relativeCoord ) / dot(gradVec, gradVec);
            return float3( t, 0, 0 );
        }
    }
    return float3(0,0,0);
}

half GetFillGradientT( float3 coords, int fillType, float4 start ){
    float t = 0;
    switch( fillType ){
    case FILL_TYPE_LINEAR:
        t = saturate(coords.x); // interpolation is done in the vertex shader so shrug~
        break;
    case FILL_TYPE_RADIAL:
        half radius = start.w;
        t = saturate( length( coords ) / radius ); // start.w = radius
        break;
    }
    return t;
}

half4 GetFillColor( float3 fillCoords, int fillType, float4 start, half4 color, half4 colorEnd ){
    if( fillType == FILL_TYPE_NONE ){
        return color;
    } else {
        half t = GetFillGradientT( fillCoords, fillType, start );
        return lerp( color, colorEnd, t );
    }
}

// parameters for selection outlines
#ifdef SCENE_VIEW_OUTLINE_MASK
	int _ObjectId;
	int _PassValue;
#endif
#ifdef SCENE_VIEW_PICKING
	uniform float4 _SelectionID;
#endif

// used for the final output. supports branching based on opaque vs transparent and outline functions
inline float4 ShapesOutput( float4 shape_color, float shape_mask ){
    float4 outColor = float4(shape_color.rgb, shape_mask * shape_color.a);
    
    clip(outColor.a - VERY_SMOL);
    
    #ifdef ADDITIVE
        outColor.rgb *= outColor.a; // additive fade base on alpha
    #endif
    #ifdef MULTIPLICATIVE
        outColor.rgb = 1 + outColor.a * ( outColor.rgb - 1 ); // lerp(1,b,t) = 1 + t(b - 1);
    #endif
    
    #if defined(SCENE_VIEW_OUTLINE_MASK) || defined(SCENE_VIEW_PICKING)
        clip(shape_mask - 0.5 + VERY_SMOL); // Don't take color into account
    #endif 
    
    #if defined( SCENE_VIEW_OUTLINE_MASK )
        return float4(_ObjectId, _PassValue, 1, 1);
    #elif defined( SCENE_VIEW_PICKING )
        return _SelectionID;
    #else
        return outColor; // Render mesh
    #endif
}