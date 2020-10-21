// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

// here you can configure the precision of the fragment shader output.
// if you are on handheld devices, it's recommended to use half4 or fixed4.
// only use float4 if you really have to, such as if you are relying a lot on massive HDR values
// valid values:
// fixed4  = 11 bit, cheap and very low precision output, range of –2 to +2 and 1/256th precision
// half4   = 16 bit, range of –60000 to +60000, with about 3 decimal digits of precision
// float4  = 32 bit, full floating point precision
#define FRAG_OUTPUT_V4 half4

// 0 = off (no anti-aliasing apart from MSAA, if enabled in your project)
// 1 = approximate, usually good enough (uses the approximate partial derivative of fwidth for anti-aliasing) 
// 2 = higher quality, mathematically correct. primarily handles diagonals better (uses more precise partial derivative calculations)
#define LOCAL_ANTI_ALIASING_QUALITY 2

// 0 = direct barycentric interpolation of colors per vertex
//     • super cheap, very approximate and prone to triangular artifacts
//     • playstation 1 energy
// 1 = barycentric interpolation of UVs, bilinear interpolation in the fragment shader
//     • this gets you like 80% there, most games settle here
//     • only use quality above this one if you really need to
//     • or if you are as pretentious as me with colors
// 2 = 2D only, Z plane only, inverse barycentric interpolation in the fragment shader based on vertex positions.
//     • when restricted to the Z plane this is a mathematically correct method.
//     • numerically unstable when not aligned with the Z plane,
//     • utterly and completely broken on the X plane or the Y plane
//     • like, it goes invisible and I don't even know why. I think we're dividing by 0 or something idk
// 3 = full 3D inverse barycentric interpolation in the fragment shader based on vertex positions.
//     • when all points are planar, this is a mathematically correct method
//     • skew quads are interpolated in a best-fit 2D projection
//     • the shader gets like way more expensive but the colors are nice and you can look at it and go "nice"
#define QUAD_INTERPOLATION_QUALITY 1

// this is used for noots, a unit (in addition to meters and pixels) useful for resolution-independent sizing
// a noot is proportional to the shortest dimension of your resolution (note: this is unrelated to physical size)
// converting noots to pixels is min(resolutionPxWidth,resolutionPxHeight) * (noots / NOOTS_ACROSS_SCREEN) 
// you can specify how big a single noot is here, though, I recommended leaving it at the default value of 100
// if NOOTS_ACROSS_SCREEN = 100, it means 1 noot is 1% of the screen (like vmin in CSS) 
// if NOOTS_ACROSS_SCREEN = 1,   it means 1 noot is 100% of the screen
#define NOOTS_ACROSS_SCREEN 100