using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Cone" )]
	public class Cone : ShapeRenderer {

		[SerializeField] float radius = 1;
		public float Radius {
			get => radius;
			set => SetFloatNow( ShapesMaterialUtils.propRadius, radius = Mathf.Max( 0f, value ) );
		}
		[SerializeField] float length = 1.5f;
		public float Length {
			get => length;
			set => SetFloatNow( ShapesMaterialUtils.propLength, length = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace sizeSpace = Shapes.ThicknessSpace.Meters;
		[System.Obsolete( "this property is obsolete I'm sorry! this was a typo, please use SizeSpace instead!", true )]
		public ThicknessSpace RadiusSpace {
			get => SizeSpace;
			set => SizeSpace = value;
		}
		public ThicknessSpace SizeSpace {
			get => sizeSpace;
			set => SetIntNow( ShapesMaterialUtils.propSizeSpace, (int)( sizeSpace = value ) );
		}
		[SerializeField] bool fillCap = true;
		public bool FillCap {
			get => fillCap;
			set {
				fillCap = value;
				UpdateMesh( true );
			}
		}

		protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetFloat( ShapesMaterialUtils.propLength, length );
			SetInt( ShapesMaterialUtils.propSizeSpace, (int)sizeSpace );
		}

		protected override void ShapeClampRanges() {
			radius = Mathf.Max( 0f, radius );
			length = Mathf.Max( 0f, length );
		}

		public override bool HasScaleModes => false;
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matCone[BlendMode] };
		protected override Mesh GetInitialMeshAsset() => fillCap ? ShapesMeshUtils.ConeMesh : ShapesMeshUtils.ConeMeshUncapped;

		protected override Bounds GetBounds() {
			if( sizeSpace != ThicknessSpace.Meters )
				return new Bounds( Vector3.zero, Vector3.one );
			return new Bounds( Vector3.zero, new Vector3( radius * 2, radius * 2, length ) );
		}

	}

}