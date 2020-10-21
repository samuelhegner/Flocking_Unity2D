using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteAlways]
	public class ShapeGroup : MonoBehaviour {

		public static int shapeGroupsInScene = 0;

		[ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )]
		[SerializeField] Color color = Color.white;

		// this is because in OnDisable, this component reads as still being enabled by the child shapes
		// so, we've got an additional lil noot to make sure things do a correct upon the things
		[System.NonSerialized] bool isEnabled = false;
		public bool IsEnabled => isEnabled;

		void OnEnable() {
			shapeGroupsInScene++;
			isEnabled = true;
			UpdateChildShapes();
		}

		void OnDisable() {
			shapeGroupsInScene--;
			isEnabled = false;
			UpdateChildShapes();
		}

		public Color Color {
			get => color;
			set {
				color = value;
				UpdateChildShapes();
			}
		}

		void OnValidate() => UpdateChildShapes();

		void UpdateChildShapes() {
			ShapeRenderer[] shapes = GetComponentsInChildren<ShapeRenderer>();
			if( shapes != null )
				foreach( ShapeRenderer shape in shapes )
					shape.UpdateAllMaterialProperties();
		}
	}

}