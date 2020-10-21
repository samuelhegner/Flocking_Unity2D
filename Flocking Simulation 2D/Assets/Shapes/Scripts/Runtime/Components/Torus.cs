using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Torus" )]
	public class Torus : ShapeRenderer {

		[SerializeField] float radius = 1;
		public float Radius {
			get => radius;
			set => SetFloatNow( ShapesMaterialUtils.propRadius, radius = Mathf.Max( 0f, value ) );
		}
		[SerializeField] float thickness = 0.5f;
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = Mathf.Max( 0f, value ) );
		}
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}
		[SerializeField] ThicknessSpace radiusSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace RadiusSpace {
			get => radiusSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( radiusSpace = value ) );
		}

		protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propRadius, radius );
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetInt( ShapesMaterialUtils.propRadiusSpace, (int)radiusSpace );
		}

		protected override void ShapeClampRanges() {
			radius = Mathf.Max( 0f, radius );
			thickness = Mathf.Max( 0f, thickness );
		}

		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matTorus[BlendMode] };
		protected override Mesh GetInitialMeshAsset() => ShapesMeshUtils.TorusMesh;

		protected override Bounds GetBounds() {
			if( radiusSpace != ThicknessSpace.Meters )
				return new Bounds( default, Vector3.one );
			// presume 0 world space padding when pixels or noots are used
			float padding = thicknessSpace == ThicknessSpace.Meters ? thickness : 0f;
			float xySize = radius * 2 + padding;
			return new Bounds( Vector3.zero, new Vector3( xySize, xySize, padding ) );
		}

	}

}