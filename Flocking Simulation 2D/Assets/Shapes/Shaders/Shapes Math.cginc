// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

// constants
#define TAU 6.28318530718
#define VERY_SMOL 0.000001

#define THICKN_SPACE_METERS 0
#define THICKN_SPACE_PIXELS 1
#define THICKN_SPACE_NOOTS 2

#define SCALE_MODE_UNIFORM 0
#define SCALE_MODE_COORDINATE 1

#define DASH_TYPE_BASIC 0
#define DASH_TYPE_ANGLED 1
#define DASH_TYPE_ROUND 2

#define DASH_SPACE_FIXED_COUNT -2
#define DASH_SPACE_RELATIVE -1
#define DASH_SPACE_METERS 0

#define DASH_SNAP_OFF 0
#define DASH_SNAP_TILING 1
#define DASH_SNAP_ENDTOEND 2

#define FILL_TYPE_NONE -1
#define FILL_TYPE_LINEAR 0
#define FILL_TYPE_RADIAL 1

#define FILL_SPACE_LOCAL 0
#define FILL_SPACE_WORLD 1

// remap functions
inline float InverseLerp( float a, float b, float v ) {
	return (v - a) / (b - a);
}
inline float2 InverseLerp( float2 a, float2 b, float2 v ) {
	return (v - a) / (b - a);
}
inline float3 InverseLerp( float3 a, float3 b, float v ) {
	return (v - a) / (b - a);
}
float2 Remap( float2 iMin, float2 iMax, float2 oMin, float2 oMax, float2 v ) {
	float2 t = InverseLerp( iMin, iMax, v );
	return lerp( oMin, oMax, t );
}
inline float Round( float a, float divs ){
	return round(a*divs)/divs;
}

// vector utils
float Determinant( in float2 a, in float2 b ) {
    return a.x * b.y - a.y * b.x;
}
float2 Rotate( float2 v, float ang ){
	float2 a = float2( cos(ang), sin(ang) );
	return float2(
		a.x * v.x - a.y * v.y,
		a.y * v.x + a.x * v.y
	);
}
inline half2 AngToDir( half ang ){
    return half2(cos(ang),sin(ang));
}
inline half DirToAng( half2 dir ){
    return atan2( dir.y, dir.x );
}
inline float2 Rotate90Left( in float2 v ){
	return float2( -v.y, v.x );
}
inline float2 Rotate90Right( in float2 v ){
	return float2( v.y, -v.x );
}
inline float3 Rotate90Left( in float3 v ){
	return float3( -v.y, v.x, 0 );
}
inline float3 Rotate90Right( in float3 v ){
	return float3( v.y, -v.x, 0 );
}
void GetDirMag( in float2 v, out float2 dir, out float mag ){
	mag = length( v );
	dir = v / mag; // Normalize
}
void GetDirMag( in float3 v, out float3 dir, out float mag ){
	mag = length( v );
	dir = v / mag; // Normalize
}

// color/value utils
inline float4 GammaCorrectVertexColorsIfNeeded( in float4 color ){
    #ifdef UNITY_COLORSPACE_GAMMA
        return color;
    #else
        return float4( GammaToLinearSpace(color.rgb), color.a ); 
    #endif
}
float PxNoise( float2 uv ){
    float2 s = uv + 0.2127+uv.x*0.3713*uv.y;
    float2 r = 4.789*sin(489.123*s);
    return frac(r.x*r.y*(1+s.x));
}



// modified version of http://iquilezles.org/www/articles/ibilinear/ibilinear.htm
float2 InvBilinear( in float2 p, in float2 a, in float2 b, in float2 c, in float2 d ) {
    float2 res = float2(-1, -1);

    float2 e = d-a;
    float2 f = b-a;
    float2 g = a-d+c-b;
    float2 h = p-a;
        
    float k2 = Determinant( g, f );
    float k1 = Determinant( e, f ) + Determinant( h, g );
    float k0 = Determinant( h, e );
    
    // if edges are parallel, this is a linear equation. Do not this test here though, do
    // it in the user code
    if( abs(k2)<0.001 ) {
        float v = -k0/k1;
        float u  = (h.x*k1+f.x*k0) / ((e.x*k1-g.x*k0));
        /*if( v>0.0 && v<1.0 && u>0.0 && u<1.0 )*/  res = float2( u, v );
    } else {
        // otherwise, it's a quadratic
        float w = sqrt(max(0, k1*k1 - 4.0*k0*k2));

        float ik2 = 0.5/k2;
        float v = (-k1 - w)*ik2;
        if( v<0.0 || v>1.0 )
            v = (-k1 + w)*ik2;
        float u = (h.x - f.x*v)/(e.x + g.x*v);
        res = saturate(float2( u, v ));
    }
    
    return res;
}

// partial derivative shenanigans
#if LOCAL_ANTI_ALIASING_QUALITY != 0
inline float PD( float value ){
	#if LOCAL_ANTI_ALIASING_QUALITY == 2
		float2 pd = float2(ddx(value), ddy(value));
		return sqrt( dot( pd, pd ) );
	#endif
	#if LOCAL_ANTI_ALIASING_QUALITY == 1
		return fwidth( value );
	#endif
}
#endif

inline float StepThresholdPD( float value, float pd ) {
    return saturate( value / max( 0.00001, pd ) + 0.5 ); // sooooooooooooo this is complicated, whether it should be +0 or +.5
}
inline float StepThresholdPDAAOffset( float value, float pd, float aaOffset ) {
    return saturate( value / max( 0.00001, pd ) + aaOffset );
}
inline float StepAA( float value ) {
    #if LOCAL_ANTI_ALIASING_QUALITY == 0
        return step(0,value);
    #else
        return StepThresholdPD( value, PD( value ) );
	#endif
}

inline float StepAA( float thresh, float value ){
	return StepAA( value - thresh );
}

inline float SdfToMask( float value, float thresh ){
    float sdf = value - thresh;
    return 1-StepAA( sdf );
}

#if LOCAL_ANTI_ALIASING_QUALITY != 0
float StepAAPdThresh( float thresh, float value ){
    float pd = PD(thresh);
    value = value * pd;
    return StepAA( thresh, value );
}
float StepAAManualPD( float2 coords, float sdf, float thresh, float2 pdCoordSpace ){
	float2 pdScreenSpace = pdCoordSpace * PD( coords ); // Transform uv to screen space (does not support rotations, I think)
	float pdMag = length( pdScreenSpace ); // Get the magnitude of change
	float sub = sdf - thresh;
	return 1.0-saturate( sub / pdMag );
}
inline float GetLineLocalAA( float coord, float pxCoverage, half pxOffset = 0 ){
    float ddxy = PD(coord);
    float sdf = abs(coord)-1;
    float aaOffset = saturate(InverseLerp( 0, 1.1, pxCoverage )) * 0.5; // the 1.1 here is very much a hack but it looks good okay THIN LINES ARE HARD
    return 1.0-StepThresholdPDAAOffset( sdf, ddxy, aaOffset + pxOffset );
}
#endif

inline float2 GetDir( float angleRad ) {
	return float2(cos(angleRad), sin(angleRad));
}

// sdfs
inline float SdfBox( float2 coord, float2 size ) {
    float2 q = abs(coord) - size;
    return length(max(0,q)) + min(0,max(q.x,q.y));
}


// The MIT License (for the function below)
// Copyright © 2018 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// (modified to align with a vertex on the right, have constants as inputs and using some of my helper functions)
float SdfNgon( half tauOverN, half apothem, half halfSideLength, half angOffset, half2 p ) {
    half halfAng = tauOverN/2;
    half pAng = DirToAng(p)-halfAng-angOffset;
    half bn = tauOverN*floor( (pAng+halfAng)/tauOverN );
    half2 cs = AngToDir(bn+halfAng+angOffset);
    p = mul(p, half2x2(cs.x,-cs.y,cs.y,cs.x));
    return length(p-half2(apothem,clamp(p.y,-halfSideLength,halfSideLength)))*sign(p.x-apothem);
}

// smoothing and tweening
inline float Smooth( float x ) { // Cubic
	return x * x * (3.0 - 2.0 * x);
}
inline float Smooth2( float x ) { // Quartic
	return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
}
inline float EaseOutBack( float x, float p ){
	return lerp( x, pow( x, p )+p*(1-x), x );
}
inline float EaseOutBack2( float x ){
	return x * (3 + x*(x-3) );
}
inline float EaseOutBack3( float x ){
	return x * (4 + x*(x*x-4) );
}
inline float EaseOutBack4( float x ){
	return x * (5 + x*( x*x*x-5 ) );
}
inline float EaseOutBack5( float x ){
	return x * (6 + x*( x*x*x*x-6 ) );
}

#ifdef SCENE_VIEW_PICKING
    // long story short - when picking in the scene view, it does so by rendering a tiny 4x4 RT
    // but it does not write to _ScreenParams, so any pixel-sized objects get very incorrect values
    #define SCREEN_PARAMS float4( 4, 4, 1.25, 1.25 )
#else
    #define SCREEN_PARAMS _ScreenParams
#endif 

// space utils
inline float4 WorldToClipPos( in float3 worldPos ) {
    return mul( UNITY_MATRIX_VP, float4( worldPos, 1 ) );
}
inline float4 ViewToClipPos( in float3 viewPos ) {
    return mul( UNITY_MATRIX_P, float4( viewPos, 1 ) );
}
inline float4 LocalToClipPos( in float3 localPos ) {
    return UnityObjectToClipPos( float4( localPos, 1 ) );
}
inline float3 LocalToWorldPos( in float3 localPos ){
    return mul( UNITY_MATRIX_M, float4( localPos, 1 )).xyz; 
}
/*inline float3 LocalToViewPos( in float3 localPos ){
    return UnityObjectToViewPos( localPos ); // mul( UNITY_MATRIX_MV, float4( localPos, 1 )).xyz; 
}
inline float3 LocalToViewVec( in float3 localVec ){
    return mul( (float3x3)UNITY_MATRIX_MV, localVec ).xyz; // Unity stop warning about this pls this is valid :c
}*/

inline float3 WorldToLocalVec( in float3 worldVec ) {
    return mul((float3x3)unity_WorldToObject, worldVec);
}
inline float3 LocalToWorldVec( in float3 localVec ){
    return mul( (float3x3)UNITY_MATRIX_M, localVec ); 
}
inline float3 CameraToWorldVec( float3 camVec ){
    return mul( (float3x3)unity_CameraToWorld, camVec );
}
float2 WorldToScreenSpace( float3 worldPos ){
    float4 clipSpace = UnityObjectToClipPos( float4( worldPos, 1 ) );
    float2 normalizedScreenspace = clipSpace.xy / clipSpace.w;
    return 0.5*(normalizedScreenspace+1.0) * SCREEN_PARAMS.xy;
}
float2 WorldToScreenSpaceNormalized( float3 worldPos ){
    float4 clipSpace = WorldToClipPos( worldPos );
    return clipSpace.xy / clipSpace.w;
}

float WorldToPixelDistance( float3 worldPosA, float3 worldPosB ){
    float2 pxNrmA = WorldToScreenSpaceNormalized( worldPosA );
	float2 pxNrmB = WorldToScreenSpaceNormalized( worldPosB );
	float2 diff = (pxNrmA - pxNrmB) * SCREEN_PARAMS.xy;
	return length( diff )*0.5;
}
inline float NootsToPixels( in float noots ){
    return min( _ScreenParams.x, _ScreenParams.y ) * ( noots / NOOTS_ACROSS_SCREEN );
}
inline float PixelsToNoots( in float pixels ){
    return (NOOTS_ACROSS_SCREEN * pixels) / min( _ScreenParams.x, _ScreenParams.y );
}
inline float3 GetCameraForwardDirection(){
    return CameraToWorldVec( float3(0,0,1) );
}

// camera stuff
inline bool IsOrthographic(){
    return unity_OrthoParams.w == 1;
}
inline float3 DirectionToNearPlanePos( float3 pt ){
    if( IsOrthographic() ){
        return -GetCameraForwardDirection();
    } else {
        return normalize( _WorldSpaceCameraPos - pt );
    }
}

// scale
inline half3 GetObjectScale(){
    half3x3 m = (half3x3)UNITY_MATRIX_M;
    return half3(
        length( half3( m[0][0], m[1][0], m[2][0] ) ),
        length( half3( m[0][1], m[1][1], m[2][1] ) ),
        length( half3( m[0][2], m[1][2], m[2][2] ) )
    );
}
inline half GetUniformScale(){
    half3 s = GetObjectScale();
	return ( s.x + s.y + s.z ) / 3;
}

// line utils
inline void ConvertToPixelThickness( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace, out float pxPerMeter, out float pxWidth ){

    // calculate pixels per meter
	pxPerMeter = WorldToPixelDistance( vertOrigin, vertOrigin + normal ); // 1 unit in world space
	
	// figure out target width in pixels
	switch( thicknessSpace ){
	    case THICKN_SPACE_METERS:
	        pxWidth = thickness*pxPerMeter; // this specifically should not have the + extraWidth
	        break;
	    case THICKN_SPACE_PIXELS:
	        pxWidth = thickness;
	        break;
        case THICKN_SPACE_NOOTS:
            pxWidth = NootsToPixels( thickness );
            break;
        default:
            pxWidth = 0;
            break;
    }
}

inline half OtherSpaceToMeters( half value, half pxPerMeter, int thicknessSpace ){
    switch( thicknessSpace ){
	    case THICKN_SPACE_METERS:
	        return value;
	    case THICKN_SPACE_PIXELS:
	        return value / pxPerMeter;
        case THICKN_SPACE_NOOTS:
            return NootsToPixels( value ) / pxPerMeter;
        default:
            return 0;
    }
}

inline half MetersToOtherSpace( half meters, half pxPerMeter, int thicknessSpace ){
    switch( thicknessSpace ){
	    case THICKN_SPACE_METERS:
	        return meters;
	    case THICKN_SPACE_PIXELS:
	        return meters * pxPerMeter;
        case THICKN_SPACE_NOOTS:
            return PixelsToNoots( meters * pxPerMeter );
        default:
            return 0;
    }
}



struct LineWidthData{
    half thicknessPixelsTarget; // 1 when thicker than 1 px, px thickness when smaller
    half thicknessMeters; // vertex position thickness. includes LAA padding
    half aaPaddingScale; // multiplier used to correct UVs for LAA padding
    half pxPerMeter; // might be useful idk
};


struct LineDashData{
    half coord;
    half spacePerPeriod;
    half thicknessPerPeriod;
};

#if LOCAL_ANTI_ALIASING_QUALITY == 0
	#define AA_PADDING_PX 0
#else
	#define AA_PADDING_PX 2
#endif

inline void GetPaddingData( float thicknessPixelsTarget, out float aaPaddingScale, out float pxWidthVert ){
    // for vertex width, we need to clamp at 1px wide to prevent wandering ants and we don't want ants now do we
    pxWidthVert = max( 1, thicknessPixelsTarget+AA_PADDING_PX);
    aaPaddingScale = pxWidthVert / max( VERY_SMOL, thicknessPixelsTarget ); // how much extra we got from the padding, as a multiplier
}


inline LineWidthData GetScreenSpaceWidthData( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace ){
    LineWidthData data;
    ConvertToPixelThickness( vertOrigin, normal, thickness, thicknessSpace, /*out*/ data.pxPerMeter, /*out*/ data.thicknessPixelsTarget );
	
	float pxWidthVert;
	GetPaddingData( data.thicknessPixelsTarget, /*out*/ data.aaPaddingScale, /*out*/ pxWidthVert );
	
	// when using pixel size, scale to match pixels
	data.thicknessMeters = pxWidthVert / data.pxPerMeter; // clamps at 1px wide, then converts to meters
    
    return data;
}

inline LineWidthData GetScreenSpaceWidthDataSimple( float3 vertOrigin, float3 normal, float thickness, int thicknessSpace ){
    LineWidthData data;
    ConvertToPixelThickness( vertOrigin, normal, thickness, thicknessSpace, /*out*/ data.pxPerMeter, /*out*/ data.thicknessPixelsTarget );
    float pxWidthVert = max( 1, data.thicknessPixelsTarget );
    data.aaPaddingScale = 1; 
	data.thicknessMeters = pxWidthVert / data.pxPerMeter; // clamps at 1px wide, then converts to meters
    return data;
}

// this works in normalized space, repeating integers for every period
inline void ApplyDashMask( inout half shape_mask, LineDashData dashData, half coordAcross, int type ){
    
    half spacePerPeriod = dashData.spacePerPeriod;

	if( spacePerPeriod >= 1.0-VERY_SMOL ){
		shape_mask = 0; // pretty much just space, make invisible
		// the flipside of this (only dashes, no space) 
		// is actually not gapless for rounded dashes by design,
		// so we can't just return solid/no masking naively
    } else {
        half thicknessPerPeriod = dashData.thicknessPerPeriod;
        half2 coord = half2( coordAcross, dashData.coord );
        
        if( type == DASH_TYPE_ANGLED )
            coord.y += 0.5*coord.x*thicknessPerPeriod; // 45° angle skewing        
		half dashSdf = abs(frac(coord.y) * 2 - 1); // triangle wave
		dashSdf = InverseLerp( spacePerPeriod, 1, dashSdf ); // convert to SDF matching dash ratio
		
		half dashMask;
		if( type == DASH_TYPE_ROUND ){
            half dashPerPeriod = 1.0-spacePerPeriod;
            half dashPerThickness = dashPerPeriod/thicknessPerPeriod;
		    half lenCoord = 1.0 - dashSdf*dashPerThickness;
		    half2 roundnoot = half2(saturate(lenCoord), coord.x);
            half sdfRound = length(half2(lenCoord, coord.x))-1;
            half maskRounds = 1.0-saturate( sdfRound / fwidth(sdfRound) + 0.5 ); // 0.5 offset to center on pixel bounds
            half maskFill = saturate( (dashSdf - 1/dashPerThickness) / fwidth(dashSdf) + 0.5 ); // 0.5 offset to center on pixel bounds
            dashMask = max(maskFill, maskRounds);
		} else {
            dashMask = saturate( dashSdf / fwidth(dashSdf) + 0.5 ); // 0.5 offset to center on pixel bounds		
		}
		
		shape_mask = min(shape_mask, dashMask);
	}
}

inline half DashSnap( half periodCount, half spacePerPeriod, int snapMode ){
	switch( snapMode ){
		case DASH_SNAP_OFF:
			return periodCount;
		case DASH_SNAP_TILING:
			return max( 1, round( periodCount ) );
		case DASH_SNAP_ENDTOEND:
			return max( 1, round( periodCount + spacePerPeriod ) ) - spacePerPeriod;
		default:
			return 0;
	}
}

inline LineDashData GetDashCoordinates( half size, half spacing, half dist, half distTotal, half thickness, int thicknessSpace, half pxPerMeter, half offset, int dashSpace, int snap ){

    LineDashData dashData;
    
    // dist and dist total are both in meters, so we need to convert them to match if we're using relative coords
    if( dashSpace == DASH_SPACE_RELATIVE ) {
        dist = MetersToOtherSpace( dist, pxPerMeter, thicknessSpace );
        distTotal = MetersToOtherSpace( distTotal, pxPerMeter, thicknessSpace );
        thickness = MetersToOtherSpace( thickness, pxPerMeter, thicknessSpace );
    }
    
    // first, convert to dash count, to leave non-normalized space land asap because we hate it
    bool fixedCount = dashSpace == DASH_SPACE_FIXED_COUNT;
    half periodCount, spacePerPeriod;
    if( fixedCount ){
        spacePerPeriod = spacing;
        periodCount = DashSnap( size, spacePerPeriod, snap );
    } else {
        half rawPeriod = (size + spacing);
	    spacePerPeriod = spacing / rawPeriod;
	    periodCount = DashSnap( distTotal / rawPeriod, spacePerPeriod, snap );
    }
    
    dashData.spacePerPeriod = spacePerPeriod;
    dashData.thicknessPerPeriod = (thickness*periodCount) / (distTotal);
    
    half t = dist / distTotal; // normalized longitudinal coord
    half dashPerPeriod = 1-spacePerPeriod;
    dashData.coord = t * periodCount - offset - dashPerPeriod/2;
    
    return dashData;
}



