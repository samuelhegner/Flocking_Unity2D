using System;
using Shapes;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapeFillExtensions {
		internal static int GetShaderFillModeInt( this ShapeFill fill ) => fill != null ? (int)fill.type : ShapeFill.FILL_NONE;
	}

	[Serializable]
	public class ShapeFill {

		public const int FILL_NONE = -1;

		public FillType type = FillType.LinearGradient;
		public FillSpace space = FillSpace.Local;

		public Color colorStart = Color.black;
		public Color colorEnd = Color.white;

		public Vector3 linearStart = Vector3.zero;
		public Vector3 linearEnd = Vector3.up;

		public Vector3 radialOrigin = Vector3.zero;
		public float radialRadius = 1f;

		public static ShapeFill CreateLinear( Vector3 start, Vector3 end, Color colorStart, Color colorEnd, FillSpace space ) =>
			new ShapeFill {
				type = FillType.LinearGradient,
				linearStart = start,
				linearEnd = end,
				colorStart = colorStart,
				colorEnd = colorEnd,
				space = space
			};

		public static ShapeFill CreateRadial( Vector3 origin, float radius, Color colorInner, Color colorOuter, FillSpace space ) =>
			new ShapeFill {
				type = FillType.RadialGradient,
				radialOrigin = origin,
				radialRadius = radius,
				colorStart = colorInner,
				colorEnd = colorOuter,
				space = space
			};

		internal Vector4 GetShaderStartVector() {
			if( type == FillType.LinearGradient ) return linearStart;
			return new Vector4( radialOrigin.x, radialOrigin.y, radialOrigin.z, radialRadius );
		}

		// used for editor stuff only
		#if UNITY_EDITOR
		bool WorldSpace => space == FillSpace.World;
		public Vector3 GetRadialOriginWorld( Transform tf ) => GetWorldPos( tf, radialOrigin );
		public void SetRadialOriginWorld( Transform tf, Vector3 worldOrigin ) => SetWorldPos( tf, ref radialOrigin, worldOrigin );
		public Vector3 GetLinearStartWorld( Transform tf ) => GetWorldPos( tf, linearStart );
		public void SetLinearStartWorld( Transform tf, Vector3 worldOrigin ) => SetWorldPos( tf, ref linearStart, worldOrigin );
		public Vector3 GetLinearEndWorld( Transform tf ) => GetWorldPos( tf, linearEnd );
		public void SetLinearEndWorld( Transform tf, Vector3 worldOrigin ) => SetWorldPos( tf, ref linearEnd, worldOrigin );
		Vector3 GetWorldPos( Transform tf, Vector3 coord ) => WorldSpace ? coord : tf.TransformPoint( coord );

		void SetWorldPos( Transform tf, ref Vector3 field, Vector3 worldValue ) {
			field = WorldSpace ? worldValue : tf.InverseTransformPoint( worldValue );
		}

		public float GetRadialWorldRadius( Transform tf ) => WorldSpace ? radialRadius : tf.lossyScale.x * radialRadius;

		public void SetRadialWorldRadius( Transform tf, float newWorldRadius ) {
			radialRadius = WorldSpace ? newWorldRadius : newWorldRadius / tf.lossyScale.x;
		}
		#endif

	}

}