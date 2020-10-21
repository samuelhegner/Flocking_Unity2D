using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Cuboid" )]
	public class Cuboid : ShapeRenderer {

		[SerializeField] Vector3 size = Vector3.one;
		public Vector3 Size {
			get => size;
			set => SetVector3Now( ShapesMaterialUtils.propSize, size = value );
		}
		[SerializeField] ThicknessSpace sizeSpace = ThicknessSpace.Meters;
		public ThicknessSpace SizeSpace {
			get => sizeSpace;
			set => SetIntNow( ShapesMaterialUtils.propSizeSpace, (int)( sizeSpace = value ) );
		}

		protected override void SetAllMaterialProperties() {
			SetVector3( ShapesMaterialUtils.propSize, size );
			SetInt( ShapesMaterialUtils.propSizeSpace, (int)sizeSpace );
		}

		public override bool HasScaleModes => false;
		protected override void ShapeClampRanges() => size = Vector3.Max( default, size );
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matCuboid[BlendMode] };
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.CuboidMesh;

		protected override Bounds GetBounds() {
			if( sizeSpace != ThicknessSpace.Meters )
				return new Bounds( default, Vector3.one );
			return new Bounds( default, size );
		}

	}

}