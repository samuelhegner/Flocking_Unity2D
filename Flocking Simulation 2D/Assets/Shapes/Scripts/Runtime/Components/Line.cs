using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Line" )]
	public class Line : ShapeRenderer {

		public enum LineColorMode {
			Single,
			Double
		}

		public Vector3 this[ int i ] {
			get => i > 0 ? End : Start;
			set => _ = i > 0 ? End = value : Start = value;
		}

		// also called alignment for 2D lines
		[SerializeField] LineGeometry geometry = LineGeometry.Billboard;
		public LineGeometry Geometry {
			get => geometry;
			set {
				geometry = value;
				SetIntNow( ShapesMaterialUtils.propAlignment, (int)geometry );
				UpdateMesh( true );
				UpdateMaterial();
				ApplyProperties();
			}
		}
		[SerializeField] LineColorMode colorMode = LineColorMode.Single;
		public LineColorMode ColorMode {
			get => colorMode;
			set => SetColorNow( ShapesMaterialUtils.propColorEnd, ( colorMode = value ) == LineColorMode.Double ? colorEnd : base.Color );
		}
		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColorNow( ShapesMaterialUtils.propColorEnd, colorEnd = value );
			}
		}
		public Color ColorStart {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorEnd = Color.white;
		public Color ColorEnd {
			get => colorEnd;
			set => SetColorNow( ShapesMaterialUtils.propColorEnd, colorEnd = value );
		}
		[SerializeField] Vector3 start = Vector3.zero;
		public Vector3 Start {
			get => start;
			set => SetVector3Now( ShapesMaterialUtils.propPointStart, start = value );
		}
		[SerializeField] Vector3 end = Vector3.right;
		public Vector3 End {
			get => end;
			set => SetVector3Now( ShapesMaterialUtils.propPointEnd, end = value );
		}
		[SerializeField] float thickness = 0.125f;
		public float Thickness {
			get => thickness;
			set {
				if( dashStyle.space == DashSpace.Relative )
					SetFloat( ShapesMaterialUtils.propDashSize, dashStyle.GetNetAbsoluteSize( dashed, value ) );
				SetFloatNow( ShapesMaterialUtils.propThickness, thickness = value );
			}
		}
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}
		[SerializeField] LineEndCap endCaps = LineEndCap.Round;
		public LineEndCap EndCaps {
			get => endCaps;
			set {
				endCaps = value;
				UpdateMaterial();
			}
		}
		[SerializeField] bool matchDashSpacingToSize = true;
		[SerializeField] bool dashed = false;
		public bool Dashed {
			get => dashed;
			set {
				dashed = value;
				SetAllDashValues( now: true );
			}
		}

		[SerializeField] DashStyle dashStyle = DashStyle.DefaultDashStyleLine;
		public float DashSize {
			get => dashStyle.size;
			set {
				dashStyle.size = value;
				float netDashSize = dashStyle.GetNetAbsoluteSize( dashed, thickness );
				if( matchDashSpacingToSize )
					SetFloat( ShapesMaterialUtils.propDashSpacing, GetNetDashSpacing() );
				SetFloatNow( ShapesMaterialUtils.propDashSize, netDashSize );
			}
		}
		public float DashSpacing {
			get => dashStyle.spacing;
			set {
				dashStyle.spacing = value;
				SetFloatNow( ShapesMaterialUtils.propDashSpacing, GetNetDashSpacing() );
			}
		}
		public float DashOffset {
			get => dashStyle.offset;
			set => SetFloatNow( ShapesMaterialUtils.propDashOffset, dashStyle.offset = value );
		}
		public DashSpace DashSpace {
			get => dashStyle.space;
			set {
				SetInt( ShapesMaterialUtils.propDashSpace, (int)( dashStyle.space = value ) );
				SetFloatNow( ShapesMaterialUtils.propDashSize, dashStyle.GetNetAbsoluteSize( dashed, thickness ) );
			}
		}
		public DashSnapping DashSnap {
			get => dashStyle.snap;
			set => SetIntNow( ShapesMaterialUtils.propDashSnap, (int)( dashStyle.snap = value ) );
		}
		public DashType DashType {
			get => dashStyle.type;
			set => SetIntNow( ShapesMaterialUtils.propDashType, (int)( dashStyle.type = value ) );
		}

		void SetAllDashValues( bool now ) {
			float netDashSize = dashStyle.GetNetAbsoluteSize( dashed, thickness );
			if( Dashed ) {
				SetFloat( ShapesMaterialUtils.propDashSpacing, GetNetDashSpacing() );
				SetFloat( ShapesMaterialUtils.propDashOffset, dashStyle.offset );
				SetInt( ShapesMaterialUtils.propDashSpace, (int)dashStyle.space );
				SetInt( ShapesMaterialUtils.propDashSnap, (int)dashStyle.snap );
				if( Geometry != LineGeometry.Volumetric3D )
					SetInt( ShapesMaterialUtils.propDashType, (int)dashStyle.type ); // only applicable to non-3D lines
			}

			if( now )
				SetFloatNow( ShapesMaterialUtils.propDashSize, netDashSize );
			else
				SetFloat( ShapesMaterialUtils.propDashSize, netDashSize );
		}

		protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propPointStart, start );
			SetVector3( ShapesMaterialUtils.propPointEnd, end );
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
			SetColor( ShapesMaterialUtils.propColorEnd, colorMode == LineColorMode.Double ? colorEnd : base.Color );
			SetAllDashValues( now: false );
		}

		float GetNetDashSpacing() {
			if( matchDashSpacingToSize && DashSpace == DashSpace.FixedCount )
				return 0.5f;
			return matchDashSpacingToSize ? dashStyle.GetNetAbsoluteSize( dashed, thickness ) : dashStyle.GetNetAbsoluteSpacing( dashed, thickness );
		}

		protected override Bounds GetBounds() {
			// presume 0 world space padding when pixels or noots are used
			float padding = thicknessSpace == ThicknessSpace.Meters ? thickness : 0f;
			Vector3 center = ( start + end ) / 2f;
			Vector3 size = ShapesMath.Abs( start - end ) + new Vector3( padding, padding, padding );
			return new Bounds( center, size );
		}

		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.GetLineMat( geometry, endCaps )[BlendMode] };
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.GetLineMesh( geometry, endCaps );

		protected override void ShapeClampRanges() {
			thickness = Mathf.Max( 0, thickness );
			DashSpacing = DashSpace == DashSpace.FixedCount ? Mathf.Clamp01( DashSpacing ) : Mathf.Max( 0f, DashSpacing );
		}

	}

}