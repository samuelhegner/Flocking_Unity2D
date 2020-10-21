// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "UnityCG.cginc"
#include "../Shapes.cginc"
#pragma target 3.0

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP( float4, _Color)
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorB)
UNITY_DEFINE_INSTANCED_PROP( float4, _ColorC)
UNITY_DEFINE_INSTANCED_PROP( float3, _A)
UNITY_DEFINE_INSTANCED_PROP( float3, _B)
UNITY_DEFINE_INSTANCED_PROP( float3, _C)
UNITY_INSTANCING_BUFFER_END(Props)

struct VertexInput {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct VertexOutput {
    half4 pos : SV_POSITION;
    half4 color : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VertexOutput vert (VertexInput v) {
    UNITY_SETUP_INSTANCE_ID(v);
    VertexOutput o = (VertexOutput)0;
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    half4 colorA = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
    half4 colorB = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorB);
    half4 colorC = UNITY_ACCESS_INSTANCED_PROP(Props, _ColorC);
    o.color = colorA * v.vertex.x + colorB * v.vertex.y + colorC * v.vertex.z;
    
    half3 a = UNITY_ACCESS_INSTANCED_PROP(Props, _A);
    half3 b = UNITY_ACCESS_INSTANCED_PROP(Props, _B);
    half3 c = UNITY_ACCESS_INSTANCED_PROP(Props, _C);
    v.vertex.xyz = a * v.vertex.x + b * v.vertex.y + c * v.vertex.z;
    o.pos = UnityObjectToClipPos( v.vertex );
    
    return o;
}

FRAG_OUTPUT_V4 frag( VertexOutput i ) : SV_Target {
    UNITY_SETUP_INSTANCE_ID(i);
    return ShapesOutput( i.color, 1 );
}