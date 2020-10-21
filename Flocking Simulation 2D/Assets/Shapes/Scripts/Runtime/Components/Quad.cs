using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Quad" )]
	public class Quad : ShapeRenderer {

		public enum QuadColorMode {
			Single,
			Horizontal,
			Vertical,
			PerCorner
		}

		public Vector3 this[ int index ] {
			get {
				switch( index ) {
					case 0: return A;
					case 1: return B;
					case 2: return C;
					case 3: return D;
					default:
						throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to access element {index}" );
				}
			}
			set {
				switch( index ) {
					case 0:
						A = value;
						break;
					case 1:
						B = value;
						break;
					case 2:
						C = value;
						break;
					case 3:
						D = value;
						break;
					default:
						throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to set element {index}" );
				}
			}
		}

		public Vector3 GetQuadVertex( int index ) => this[index];
		public Vector3 SetQuadVertex( int index, Vector3 value ) => this[index] = value;

		public Color GetQuadColor( int index ) {
			switch( index ) {
				case 0: return Color;
				case 1: return ColorB;
				case 2: return ColorC;
				case 3: return ColorD;
				default:
					throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to access element {index}" );
			}
		}

		public void SetQuadColor( int index, Color color ) {
			switch( index ) {
				case 0:
					Color = color;
					break;
				case 1:
					ColorB = color;
					break;
				case 2:
					ColorC = color;
					break;
				case 3:
					ColorD = color;
					break;
				default:
					throw new IndexOutOfRangeException( $"Quad only has four vertices, 0 to 3, you tried to set element {index}" );
			}
		}

		[SerializeField] QuadColorMode colorMode = QuadColorMode.Single;
		public QuadColorMode ColorMode {
			get => colorMode;
			set {
				colorMode = value;
				ApplyProperties();
			}
		}

		[SerializeField] Vector3 a = new Vector2( -0.5f, -0.5f );
		public Vector3 A {
			get => a;
			set {
				SetVector3Now( ShapesMaterialUtils.propA, a = value );
				CheckAutoSetD();
			}
		}
		[SerializeField] Vector3 b = new Vector2( -0.5f, 0.5f );
		public Vector3 B {
			get => b;
			set {
				SetVector3Now( ShapesMaterialUtils.propB, b = value );
				CheckAutoSetD();
			}
		}
		[SerializeField] Vector3 c = new Vector2( 0.5f, 0.5f );
		public Vector3 C {
			get => c;
			set {
				SetVector3Now( ShapesMaterialUtils.propC, c = value );
				CheckAutoSetD();
			}
		}
		[SerializeField] Vector3 d = new Vector2( 0.5f, -0.5f );
		public Vector3 D {
			get => d;
			set {
				if( autoSetD )
					Debug.LogWarning( "tried to set D when auto-set is enabled, you might want to turn off auto-set on this object", gameObject );
				else
					SetVector3Now( ShapesMaterialUtils.propD, d = value );
			}
		}
		#if UNITY_EDITOR
		public bool IsUsingAutoD => autoSetD;
		#endif
		[SerializeField] bool autoSetD = false;
		public Vector3 DAuto => A + ( C - B );
		void AutoSetD() => SetVector3( ShapesMaterialUtils.propD, DAuto );

		void CheckAutoSetD() {
			if( autoSetD )
				AutoSetD();
		}

		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColor( ShapesMaterialUtils.propColorB, colorB = value );
				SetColor( ShapesMaterialUtils.propColorC, colorC = value );
				SetColorNow( ShapesMaterialUtils.propColorD, colorD = value );
			}
		}
		public Color ColorLeft {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColorNow( ShapesMaterialUtils.propColorB, colorB = value );
			}
		}
		public Color ColorTop {
			get => colorB;
			set {
				SetColor( ShapesMaterialUtils.propColorB, colorB = value );
				SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
			}
		}
		public Color ColorRight {
			get => colorC;
			set {
				SetColor( ShapesMaterialUtils.propColorC, colorC = value );
				SetColorNow( ShapesMaterialUtils.propColorD, colorD = value );
			}
		}
		public Color ColorBottom {
			get => colorD;
			set {
				SetColor( ShapesMaterialUtils.propColorD, colorD = value );
				SetColorNow( ShapesMaterialUtils.propColor, color = value );
			}
		}

		public Color ColorA {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorB = Color.white;
		public Color ColorB {
			get => colorB;
			set => SetColorNow( ShapesMaterialUtils.propColorB, colorB = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorC = Color.white;
		public Color ColorC {
			get => colorC;
			set => SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] Color colorD = Color.white;
		public Color ColorD {
			get => colorD;
			set => SetColorNow( ShapesMaterialUtils.propColorD, colorD = value );
		}

		protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propA, a );
			SetVector3( ShapesMaterialUtils.propB, b );
			SetVector3( ShapesMaterialUtils.propC, c );
			if( autoSetD )
				AutoSetD();
			else
				SetVector3( ShapesMaterialUtils.propD, d );


			switch( colorMode ) {
				case QuadColorMode.Single:
					SetColor( ShapesMaterialUtils.propColorB, Color );
					SetColor( ShapesMaterialUtils.propColorC, Color );
					SetColor( ShapesMaterialUtils.propColorD, Color );
					break;
				case QuadColorMode.Horizontal:
					SetColor( ShapesMaterialUtils.propColorB, Color );
					SetColor( ShapesMaterialUtils.propColorC, colorC );
					SetColor( ShapesMaterialUtils.propColorD, colorC );
					break;
				case QuadColorMode.Vertical:
					SetColor( ShapesMaterialUtils.propColor, colorD ); // todo: this is a double-assign, it's already set before this, but to the wrong value :c
					SetColor( ShapesMaterialUtils.propColorB, colorB );
					SetColor( ShapesMaterialUtils.propColorC, colorB );
					SetColor( ShapesMaterialUtils.propColorD, colorD );
					break;
				case QuadColorMode.PerCorner:
					SetColor( ShapesMaterialUtils.propColorB, colorB );
					SetColor( ShapesMaterialUtils.propColorC, colorC );
					SetColor( ShapesMaterialUtils.propColorD, colorD );
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override bool HasScaleModes => false;
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.QuadMesh;
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matQuad[BlendMode] };

		protected override Bounds GetBounds() {
			Vector3 min = Vector3.Min( Vector3.Min( a, b ), c );
			Vector3 max = Vector3.Max( Vector3.Max( a, b ), c );
			return new Bounds( ( min + max ) / 2, ShapesMath.Abs( max - min ) );
		}

	}

}