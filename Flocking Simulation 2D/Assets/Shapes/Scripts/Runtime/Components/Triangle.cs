using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Triangle" )]
	public class Triangle : ShapeRenderer {

		public enum TriangleColorMode {
			Single,
			PerCorner
		}

		public Vector3 this[ int index ] {
			get {
				switch( index ) {
					case 0: return A;
					case 1: return B;
					case 2: return C;
					default:
						throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 2, you tried to access element {index}" );
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
					default:
						throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 2, you tried to set element {index}" );
				}
			}
		}

		public Vector3 GetTriangleVertex( int index ) => this[index];
		public Vector3 SetTriangleVertex( int index, Vector3 value ) => this[index] = value;

		public Color GetTriangleColor( int index ) {
			switch( index ) {
				case 0: return Color;
				case 1: return ColorB;
				case 2: return ColorC;
				default:
					throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 2, you tried to access element {index}" );
			}
		}

		public void SetTriangleColor( int index, Color color ) {
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
				default:
					throw new IndexOutOfRangeException( $"Triangle only has four vertices, 0 to 3, you tried to set element {index}" );
			}
		}

		[SerializeField] TriangleColorMode colorMode = TriangleColorMode.Single;
		public TriangleColorMode ColorMode {
			get => colorMode;
			set {
				colorMode = value;
				ApplyProperties();
			}
		}

		[SerializeField] Vector3 a = Vector3.zero;
		public Vector3 A {
			get => a;
			set => SetVector3Now( ShapesMaterialUtils.propA, a = value );
		}
		[SerializeField] Vector3 b = Vector3.up;
		public Vector3 B {
			get => b;
			set => SetVector3Now( ShapesMaterialUtils.propB, b = value );
		}
		[SerializeField] Vector3 c = Vector3.right;
		public Vector3 C {
			get => c;
			set => SetVector3Now( ShapesMaterialUtils.propC, c = value );
		}

		public override Color Color {
			get => color;
			set {
				SetColor( ShapesMaterialUtils.propColor, color = value );
				SetColor( ShapesMaterialUtils.propColorB, colorB = value );
				SetColorNow( ShapesMaterialUtils.propColorC, colorC = value );
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

		protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propA, a );
			SetVector3( ShapesMaterialUtils.propB, b );
			SetVector3( ShapesMaterialUtils.propC, c );
			if( colorMode == TriangleColorMode.Single ) {
				SetColor( ShapesMaterialUtils.propColorB, Color );
				SetColor( ShapesMaterialUtils.propColorC, Color );
			} else { // per corner
				SetColor( ShapesMaterialUtils.propColorB, colorB );
				SetColor( ShapesMaterialUtils.propColorC, colorC );
			}
		}

		public override bool HasScaleModes => false;
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.TriangleMesh;
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matTriangle[BlendMode] };

		protected override Bounds GetBounds() {
			Vector3 min = Vector3.Min( Vector3.Min( a, b ), c );
			Vector3 max = Vector3.Max( Vector3.Max( a, b ), c );
			return new Bounds( ( min + max ) / 2, ShapesMath.Abs( max - min ) );
		}

	}

}