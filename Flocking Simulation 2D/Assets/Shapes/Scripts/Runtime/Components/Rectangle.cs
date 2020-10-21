using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Rectangle" )]
	public class Rectangle : ShapeRenderer {

		public enum RectangleType {
			HardSolid,
			RoundedSolid,
			HardHollow,
			RoundedHollow
		}

		public enum RectangleCornerRadiusMode {
			Uniform,
			PerCorner
		}

		public bool IsHollow => type == RectangleType.HardHollow || type == RectangleType.RoundedHollow;
		public bool IsRounded => type == RectangleType.RoundedSolid || type == RectangleType.RoundedHollow;

		[SerializeField] RectPivot pivot = RectPivot.Center;
		public RectPivot Pivot {
			get => pivot;
			set {
				pivot = value;
				UpdateRectPositioningNow();
			}
		}

		[SerializeField] float width = 1f;
		public float Width {
			get => width;
			set {
				width = value;
				UpdateRectPositioningNow();
			}
		}

		[SerializeField] float height = 1f;
		public float Height {
			get => height;
			set {
				height = value;
				UpdateRectPositioningNow();
			}
		}

		[SerializeField] RectangleType type = RectangleType.HardSolid;
		public RectangleType Type {
			get => type;
			set {
				type = value;
				UpdateMaterial();
				ApplyProperties();
			}
		}


		[SerializeField] RectangleCornerRadiusMode cornerRadiusMode = RectangleCornerRadiusMode.Uniform;
		public RectangleCornerRadiusMode CornerRadiusMode {
			get => cornerRadiusMode;
			set => cornerRadiusMode = value;
		}

		[System.Obsolete( "Radius is deprecated, please use " + nameof(CornerRadius) + " instead", true )]
		public float Radius {
			get => CornerRadius;
			set => CornerRadius = value;
		}

		[SerializeField] Vector4 cornerRadii = new Vector4( 0.25f, 0.25f, 0.25f, 0.25f );
		/// <summary>
		/// Gets or sets a radius for all 4 corners when rounded
		/// </summary>
		public float CornerRadius {
			get => cornerRadii.x;
			set {
				float r = Mathf.Max( 0f, value );
				SetVector4Now( ShapesMaterialUtils.propCornerRadii, cornerRadii = new Vector4( r, r, r, r ) );
			}
		}
		/// <summary>
		/// Gets or sets a specific radius for each corner when rounded. Order is clockwise from bottom left
		/// </summary>
		public Vector4 CornerRadiii {
			get => cornerRadii;
			set => SetVector4Now( ShapesMaterialUtils.propCornerRadii, cornerRadii = new Vector4( Mathf.Max( 0f, value.x ), Mathf.Max( 0f, value.y ), Mathf.Max( 0f, value.z ), Mathf.Max( 0f, value.w ) ) );
		}

		[SerializeField] float thickness = 0.1f;
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = Mathf.Max( 0f, value ) );
		}

		void UpdateRectPositioningNow() => SetVector4Now( ShapesMaterialUtils.propRect, GetPositioningRect() );
		void UpdateRectPositioning() => SetVector4( ShapesMaterialUtils.propRect, GetPositioningRect() );

		Vector4 GetPositioningRect() {
			float xOffset = pivot == RectPivot.Corner ? 0f : -width / 2f;
			float yOffset = pivot == RectPivot.Corner ? 0f : -height / 2f;
			return new Vector4( xOffset, yOffset, width, height );
		}

		protected override void SetAllMaterialProperties() {
			if( cornerRadiusMode == RectangleCornerRadiusMode.PerCorner )
				SetVector4( ShapesMaterialUtils.propCornerRadii, cornerRadii );
			else if( cornerRadiusMode == RectangleCornerRadiusMode.Uniform )
				SetVector4( ShapesMaterialUtils.propCornerRadii, new Vector4( CornerRadius, CornerRadius, CornerRadius, CornerRadius ) );
			UpdateRectPositioning();
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
		}

		#if UNITY_EDITOR
		protected override void ShapeClampRanges() {
			cornerRadii = ShapesMath.AtLeast0( cornerRadii );
			width = Mathf.Max( 0f, width );
			height = Mathf.Max( 0f, height );
		}
		#endif

		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.GetRectMaterial( type )[BlendMode] };

		protected override Bounds GetBounds() {
			Vector2 size = new Vector2( width, height );
			Vector2 center = pivot == RectPivot.Center ? default : size / 2f;
			return new Bounds( center, size );
		}
	}

}