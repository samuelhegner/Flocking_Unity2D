using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Sphere" )]
	public class Sphere : ShapeRenderer {

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

		protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetInt( ShapesMaterialUtils.propRadiusSpace, (int)radiusSpace );
		}

		public override bool HasScaleModes => false;
		protected override void ShapeClampRanges() => radius = Mathf.Max( 0f, radius );
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matSphere[BlendMode] };
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.SphereMesh;

		protected override Bounds GetBounds() {
			if( radiusSpace != ThicknessSpace.Meters )
				return new Bounds( Vector3.zero, Vector3.one );
			return new Bounds( Vector3.zero, Vector3.one * ( radius * 2 ) );
		}

	}

}