using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static partial class Draw {

		// draw matrix states:

		static Matrix4x4 matrix = Matrix4x4.identity;
		static bool hasCustomMatrix = false;
		public static Matrix4x4 Matrix {
			get => matrix;
			set {
				matrix = value;
				hasCustomMatrix = value != Matrix4x4.identity;
			}
		}

		// draw style states:

		// globally shared style states
		public static Color Color { get; set; }
		public static CompareFunction ZTest { get; set; }
		public static float ZOffsetFactor { get; set; }
		public static int ZOffsetUnits { get; set; }
		public static ShapesBlendMode BlendMode { get; set; }
		public static ScaleMode ScaleMode { get; set; }

		// shared line & polyline states
		public static float LineThickness { get; set; }
		public static ThicknessSpace LineThicknessSpace { get; set; }

		// line states
		public static LineEndCap LineEndCaps { get; set; }
		public static LineGeometry LineGeometry { get; set; }

		// polygon states
		public static PolygonTriangulation PolygonTriangulation { get; set; }
		public static ShapeFill PolygonShapeFill { get; set; }

		// line dashes
		public static DashStyle LineDashStyle { get; set; }
		public static DashStyle RingDashStyle { get; set; }

		[System.Obsolete( "please use Draw.LineDashStyle.UniformSize or Draw.LineDashStyle.size instead", false )]
		public static float LineDashSize {
			get => LineDashStyle.UniformSize;
			set => LineDashStyle.UniformSize = value;
		}

		// polyline states
		public static PolylineGeometry PolylineGeometry { get; set; }
		public static PolylineJoins PolylineJoins { get; set; }

		// disc & ring states
		public static float DiscRadius { get; set; }
		public static DiscGeometry DiscGeometry { get; set; }
		public static float RingThickness { get; set; }
		public static ThicknessSpace RingThicknessSpace { get; set; }
		public static ThicknessSpace DiscRadiusSpace { get; set; }
		
		// regular polygon states
		public static float RegularPolygonRadius { get; set; }
		public static int RegularPolygonSideCount { get; set; }
		public static RegularPolygonGeometry RegularPolygonGeometry { get; set; }
		public static float RegularPolygonThickness { get; set; }
		public static ThicknessSpace RegularPolygonThicknessSpace { get; set; }
		public static ThicknessSpace RegularPolygonRadiusSpace { get; set; }
		public static ShapeFill RegularPolygonShapeFill { get; set; }

		// 3D shape states
		public static float SphereRadius { get; set; }
		public static ThicknessSpace SphereRadiusSpace { get; set; }
		public static ThicknessSpace CuboidSizeSpace { get; set; }
		public static ThicknessSpace TorusThicknessSpace { get; set; }
		public static ThicknessSpace TorusRadiusSpace { get; set; }
		public static ThicknessSpace ConeSizeSpace { get; set; }

		// text states
		public static TMP_FontAsset Font { get; set; }
		public static float FontSize { get; set; }
		public static TextAlign TextAlign { get; set; }

		// initialize all default values
		static Draw() => ResetAllDrawStates();

		/// <summary>
		/// Resets all static states - both style & matrix
		/// </summary>
		public static void ResetAllDrawStates() {
			ResetMatrix();
			ResetStyle();
		}

		/// <summary>
		/// Resets the matrix to Matrix4x4.identity
		/// </summary>
		public static void ResetMatrix() {
			matrix = Matrix4x4.identity;
			hasCustomMatrix = false;
		}

		/// <summary>
		/// Resets style states, but not the drawing matrix
		/// </summary>
		/// See <see cref="Draw.ResetAllDrawStates()"/> to reset everything
		public static void ResetStyle() {
			Color = Color.white;
			ZTest = CompareFunction.LessEqual;
			ZOffsetFactor = 0f;
			ZOffsetUnits = 0;
			BlendMode = ShapesBlendMode.Transparent;
			ScaleMode = ScaleMode.Uniform;
			LineThickness = 0.05f;
			LineThicknessSpace = ThicknessSpace.Meters;
			LineDashStyle = DashStyle.DefaultDashStyleLine;
			LineEndCaps = LineEndCap.Round;
			LineGeometry = LineGeometry.Billboard;
			PolygonTriangulation = PolygonTriangulation.EarClipping;
			PolygonShapeFill = new ShapeFill();
			PolylineGeometry = PolylineGeometry.Billboard;
			PolylineJoins = PolylineJoins.Round;
			
			// disc
			DiscGeometry = DiscGeometry.Flat2D;
			DiscRadius = 1f;
			RingThickness = 0.05f;
			RingThicknessSpace = ThicknessSpace.Meters;
			DiscRadiusSpace = ThicknessSpace.Meters;
			RingDashStyle = DashStyle.DefaultDashStyleRing;
			
			// regular polygon
			RegularPolygonRadius = 1f;
			RegularPolygonSideCount = 6;
			RegularPolygonGeometry = RegularPolygonGeometry.Flat2D;
			RegularPolygonThickness = 0.05f;
			RegularPolygonThicknessSpace = ThicknessSpace.Meters;
			RegularPolygonRadiusSpace = ThicknessSpace.Meters;
			RegularPolygonShapeFill = new ShapeFill();
			
			SphereRadius = 1f;
			SphereRadiusSpace = ThicknessSpace.Meters;
			CuboidSizeSpace = ThicknessSpace.Meters;
			TorusThicknessSpace = ThicknessSpace.Meters;
			TorusRadiusSpace = ThicknessSpace.Meters;
			ConeSizeSpace = ThicknessSpace.Meters;
			Font = ShapesAssets.Instance.defaultFont;
			FontSize = 1f;
			TextAlign = TextAlign.Center;
		}


	}

}