using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[CustomEditor( typeof(RegularPolygon) )]
	[CanEditMultipleObjects]
	public class RegularPolygonEditor : ShapeRendererEditor {

		SerializedProperty propGeometry = null;
		SerializedProperty propSides = null;
		SerializedProperty propHollow = null;
		SerializedProperty propAngle = null;
		SerializedProperty propRoundness = null;
		SerializedProperty propAngUnitInput = null;
		SerializedProperty propRadius = null;
		SerializedProperty propRadiusSpace = null;
		SerializedProperty propThickness = null;
		SerializedProperty propThicknessSpace = null;
		SerializedProperty propFill = null;
		SerializedProperty propUseFill = null;

		SceneFillEditor fillEditor;
		SceneDiscEditor discEditor; // todo: polygonal version

		public override void OnEnable() {
			base.OnEnable();
			fillEditor = new SceneFillEditor( this, propFill, propUseFill );
			discEditor = new SceneDiscEditor( this );
		}

		void OnSceneGUI() {
			bool changed = discEditor.DoSceneHandles( target as RegularPolygon );
		}

		int[] indexToPolygonPreset = { 3, 4, 5, 6, 8 };
		
		GUIContent[] sideCountTypes;
		GUIContent[] SideCountTypes =>
			sideCountTypes ?? ( sideCountTypes = new[] {
				new GUIContent( ShapesAssets.Instance.GetRegularPolygonIcon( 3 ), "Triangle" ),
				new GUIContent( ShapesAssets.Instance.GetRegularPolygonIcon( 4 ), "Square" ),
				new GUIContent( ShapesAssets.Instance.GetRegularPolygonIcon( 5 ), "Pentagon" ),
				new GUIContent( ShapesAssets.Instance.GetRegularPolygonIcon( 6 ), "Hexagon" ),
				new GUIContent( ShapesAssets.Instance.GetRegularPolygonIcon( 8 ), "Octagon" )
			} );


		public override void OnInspectorGUI() {
			base.BeginProperties( showColor: true );

			EditorGUILayout.PropertyField( propGeometry );
			ShapesUI.DrawTypeSwitchButtons( propSides, SideCountTypes, indexToPolygonPreset );
			//ShapesUI.EnumButtonRow(); // todo
			EditorGUILayout.PropertyField( propSides );
			EditorGUILayout.PropertyField( propRoundness );

			ShapesUI.FloatInSpaceField( propRadius, propRadiusSpace );

			using( new EditorGUI.DisabledScope( propHollow.boolValue == false || propHollow.hasMultipleDifferentValues ) )
				ShapesUI.FloatInSpaceField( propThickness, propThicknessSpace );
			EditorGUILayout.PropertyField( propHollow );
			ShapesUI.AngleProperty( propAngle, "Angle", propAngUnitInput, angLabelLayout );
			ShapesUI.DrawAngleSwitchButtons( propAngUnitInput );

			bool canEditInSceneView = propRadiusSpace.hasMultipleDifferentValues || propRadiusSpace.enumValueIndex == (int)ThicknessSpace.Meters;
			using( new EditorGUI.DisabledScope( canEditInSceneView == false ) )
				discEditor.GUIEditButton();

			fillEditor.DrawProperties( this );

			base.EndProperties();
		}


		static GUILayoutOption[] angLabelLayout = { GUILayout.Width( 50 ) };


	}

}