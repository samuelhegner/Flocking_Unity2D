using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesConfig {

		// whether or not inspectors should use HDR color pickers
		public const bool USE_HDR_COLOR_PICKERS = false;

		// these settings are uh, very esoteric
		// *if* you are having trouble with *many* shapes being drawn on screen at the same time,
		// making the bounds smaller using this parameter might help you optimize your game
		//
		// this is like, super technical, so please read every word very carefully below:
		// this value should be set so that *all* shapes using, for instance, the quad mesh (disc, line, rect, etc.),
		// can use *these specific bounds*, so that the bounds would encapsulate the entire shape.
		// practically, this means that these bounds should be set so that it can encapsulate the largest
		// shape you have in your project. if this is set too low, larger shapes will pop in/out of existence
		// 
		// the purpose of this is to gain some benefit in culling, but still keep the benefits of instancing
		// by default, size is set to a large value of 1 << 16 (65536), practically "turning off" frustum culling
		static Bounds VERY_LORGE_BOUNDS = new Bounds( Vector3.zero, Vector3.one * ( 1 << 16 ) );
		public static Bounds BOUNDS_QUAD_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_TRIANGLE_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_SPHERE_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_TORUS_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_CUBOID_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_CONE_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_CYLINDER_PRIMITIVE = VERY_LORGE_BOUNDS;
		public static Bounds BOUNDS_CAPSULE_PRIMITIVE = VERY_LORGE_BOUNDS;

		// default point density for polyline arcs and beziers in points per full turn
		// if set to 128, then it'll use 64 points for a 180° turn, 32 points for a 90° turn
		//
		// 16 = curves are very jagged, clearly just a bunch of straight lines in a trenchcoat, except they forgot the trenchcoat
		// 32 = curves visibly have straight segments when looking close, but appear smooth at a quick glance. (trenchcoat is now on)
		// 64 = curves generally appear smooth, except at the very sharpest of turns. recommended value.
		// 128 = curves appear smooth in pretty much all cases, beyond this is pretty wild, but I mean, if you're a wild person then go for it
		public const float POLYLINE_DEFAULT_POINTS_PER_TURN = 64;

		// default accuracy when calculating point density of bezier curves.
		// this is only used for bezier curves where you specify density rather than point count.
		// if you have mostly very simple bezier curves, you can leave this at 3.
		// if you have more complex curves, like those with widely separated control points squishing the curve,
		// then you should use at least 5 samples
		//
		// 1 = ~12% margin of error. this is the minimum value! works for the simplest curves, but generally inaccurate
		// 2 = ~4% margin of error. this is recommended, good balance between accuracy and speed
		// 3 = ~2% margin of error
		// 4 = ~1% margin of error
		public const int POLYLINE_BEZIER_ANGULAR_SUM_ACCURACY = 2;


	}


}