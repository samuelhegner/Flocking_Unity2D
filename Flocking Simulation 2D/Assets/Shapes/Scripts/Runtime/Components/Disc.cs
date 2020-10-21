using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Disc" )]
	public class Disc : ShapeRenderer {

		public enum DiscColorMode {
			Single,
			Radial,
			Angular,
			Bilinear
		}

		public bool HasThickness => type.HasThickness();
		public bool HasSector => type.HasSector();

		[SerializeField] DiscType type = DiscType.Disc;
		public DiscType Type {
			get => type;
			set {
				type = value;
				UpdateMaterial();
				ApplyProperties();
			}
		}

		[SerializeField] DiscColorMode colorMode = DiscColorMode.Single;
		public DiscColorMode ColorMode {
			get => colorMode;
			set {
				colorMode = value;
				ApplyProperties();
			}
		}
		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColor( ShapesMaterialUtils.propColorOuterStart, colorOuterStart = value );
				SetColor( ShapesMaterialUtils.propColorInnerEnd, colorInnerEnd = value );
				SetColorNow( ShapesMaterialUtils.propColorOuterEnd, colorOuterEnd = value );
			}
		}
		public Color ColorInnerStart {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorOuterStart = Color.white;
		public Color ColorOuterStart {
			get => colorOuterStart;
			set => SetColorNow( ShapesMaterialUtils.propColorOuterStart, colorOuterStart = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorInnerEnd = Color.white;
		public Color ColorInnerEnd {
			get => colorInnerEnd;
			set => SetColorNow( ShapesMaterialUtils.propColorInnerEnd, colorInnerEnd = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorOuterEnd = Color.white;
		public Color ColorOuterEnd {
			get => colorOuterEnd;
			set => SetColorNow( ShapesMaterialUtils.propColorOuterEnd, colorOuterEnd = value );
		}

		public Color ColorOuter {
			get => ColorOuterStart;
			set {
				SetColor( ShapesMaterialUtils.propColorOuterStart, colorOuterStart = value );
				SetColorNow( ShapesMaterialUtils.propColorOuterEnd, colorOuterEnd = value );
			}
		}
		public Color ColorInner {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColorNow( ShapesMaterialUtils.propColorInnerEnd, colorInnerEnd = value );
			}
		}
		public Color ColorStart {
			get => base.Color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColorNow( ShapesMaterialUtils.propColorOuterStart, colorOuterStart = value );
			}
		}
		public Color ColorEnd {
			get => colorInnerEnd;
			set {
				SetColor( ShapesMaterialUtils.propColorInnerEnd, colorInnerEnd = value );
				SetColorNow( ShapesMaterialUtils.propColorOuterEnd, colorOuterEnd = value );
			}
		}

		[SerializeField] DiscGeometry geometry = DiscGeometry.Flat2D;
		public DiscGeometry Geometry {
			get => geometry;
			set => SetIntNow( ShapesMaterialUtils.propAlignment, (int)(geometry = value) );
		}

		// in-editor serialized field, suppressing "assigned but unused field" warning
		#pragma warning disable CS0414
		[SerializeField] AngularUnit angUnitInput = AngularUnit.Degrees;
		#pragma warning restore CS0414

		[SerializeField] float angRadiansStart = 0;
		public float AngRadiansStart {
			get => angRadiansStart;
			set => SetFloatNow( ShapesMaterialUtils.propAngStart, angRadiansStart = value );
		}
		[SerializeField] float angRadiansEnd = ShapesMath.TAU * ( 3 / 8f );
		public float AngRadiansEnd {
			get => angRadiansEnd;
			set => SetFloatNow( ShapesMaterialUtils.propAngEnd, angRadiansEnd = value );
		}
		[SerializeField] float radius = 1;
		public float Radius {
			get => radius;
			set => SetFloatNow( ShapesMaterialUtils.propRadius, radius = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace radiusSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace RadiusSpace {
			get => radiusSpace;
			set => SetIntNow( ShapesMaterialUtils.propRadiusSpace, (int)( radiusSpace = value ) );
		}
		[SerializeField] float thickness = 0.5f;
		[System.Obsolete( "this property is obsolete, this was a typo! please use Thickness instead!", true )]
		public float RadiusInner {
			get => Thickness;
			set => Thickness = value;
		}
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}
		[SerializeField] ArcEndCap arcEndCaps = ArcEndCap.None;
		public ArcEndCap ArcEndCaps {
			get => arcEndCaps;
			set => SetIntNow( ShapesMaterialUtils.propRoundCaps, (int)( arcEndCaps = value ) );
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

		[SerializeField] DashStyle dashStyle = DashStyle.DefaultDashStyleRing;
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
			get => matchDashSpacingToSize ? dashStyle.size : dashStyle.spacing;
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
				SetInt( ShapesMaterialUtils.propDashType, (int)dashStyle.type );
				SetInt( ShapesMaterialUtils.propDashSnap, (int)dashStyle.snap );
			}

			if( now )
				SetFloatNow( ShapesMaterialUtils.propDashSize, netDashSize );
			else
				SetFloat( ShapesMaterialUtils.propDashSize, netDashSize );
		}

		protected override void SetAllMaterialProperties() {
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetInt( ShapesMaterialUtils.propRadiusSpace, (int)radiusSpace );
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetInt( ShapesMaterialUtils.propRoundCaps, (int)arcEndCaps );
			SetFloat( ShapesMaterialUtils.propAngStart, angRadiansStart );
			SetFloat( ShapesMaterialUtils.propAngEnd, angRadiansEnd );
			switch( ColorMode ) {
				case DiscColorMode.Single:
					SetColor( ShapesMaterialUtils.propColorOuterStart, base.Color );
					SetColor( ShapesMaterialUtils.propColorInnerEnd, base.Color );
					SetColor( ShapesMaterialUtils.propColorOuterEnd, base.Color );
					break;
				case DiscColorMode.Radial:
					SetColor( ShapesMaterialUtils.propColorOuterStart, ColorOuterStart );
					SetColor( ShapesMaterialUtils.propColorInnerEnd, base.Color );
					SetColor( ShapesMaterialUtils.propColorOuterEnd, ColorOuterStart );
					break;
				case DiscColorMode.Angular:
					SetColor( ShapesMaterialUtils.propColorOuterStart, base.Color );
					SetColor( ShapesMaterialUtils.propColorInnerEnd, ColorInnerEnd );
					SetColor( ShapesMaterialUtils.propColorOuterEnd, ColorInnerEnd );
					break;
				case DiscColorMode.Bilinear:
					SetColor( ShapesMaterialUtils.propColorOuterStart, ColorOuterStart );
					SetColor( ShapesMaterialUtils.propColorInnerEnd, ColorInnerEnd );
					SetColor( ShapesMaterialUtils.propColorOuterEnd, ColorOuterEnd );
					break;
			}

			SetAllDashValues( now: false );
		}

		float GetNetDashSpacing() {
			if( matchDashSpacingToSize && DashSpace == DashSpace.FixedCount )
				return 0.5f;
			return matchDashSpacingToSize ? dashStyle.GetNetAbsoluteSize( dashed, thickness ) : dashStyle.GetNetAbsoluteSpacing( dashed, thickness );
		}

		#if UNITY_EDITOR
		protected override void ShapeClampRanges() {
			radius = Mathf.Max( 0f, radius ); // disallow negative radius
			thickness = Mathf.Max( 0f, thickness ); // disallow negative inner radius
			if( matchDashSpacingToSize == false ) // this is a lil scary but I think it's okay it's going to be okay
				DashSpacing = DashSpace == DashSpace.FixedCount ? Mathf.Clamp01( DashSpacing ) : Mathf.Max( 0f, DashSpacing );
		}
		#endif

		protected override Material[] GetMaterials() {
			return new[] { ShapesMaterialUtils.GetDiscMaterial( type )[BlendMode] };
		}

		protected override Bounds GetBounds() {
			if( radiusSpace != ThicknessSpace.Meters )
				return new Bounds( Vector3.zero, Vector3.one );
			// presume 0 world space padding when pixels or noots are used
			float padding = thicknessSpace == ThicknessSpace.Meters ? thickness * .5f : 0f;
			float apothem = HasThickness ? radius + padding : radius;
			float size = apothem * 2;
			return new Bounds( Vector3.zero, new Vector3( size, size, 0f ) );
		}

	}

}