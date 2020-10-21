using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	//[CustomEditor( typeof( ShapeRenderer ) )]
	[CanEditMultipleObjects]
	public class ShapeRendererEditor : Editor {

		// ShapeRenderer
		protected SerializedProperty propColor;
		SerializedProperty propZTest = null;
		SerializedProperty propZOffsetFactor = null;
		SerializedProperty propZOffsetUnits = null;
		SerializedProperty propBlendMode = null;
		SerializedProperty propScaleMode = null;

		// MeshRenderer
		SerializedObject soRnd;
		SerializedProperty propSortingOrder;
		SerializedProperty propSortingLayer;

		static GUIContent blendModeGuiContent = new GUIContent(
			"Blend Mode",
			"[Opaque] does not support partial transparency, " +
			"but will write to the depth buffer and sort correctly. " +
			"For best results, use MSAA in your project to avoid aliasing " +
			"(note that it may still be aliased in the scene view)\n" +
			"\n" +
			"[Transparent] supports partial transparency, " +
			"but may not sort properly in some cases.\n" +
			"\n" +
			"[Additive] is good for glowing/brightening effects against dark backgrounds\n" +
			"\n" +
			"[Multiplicative] is good for tinting/darkening effects against bright backgrounds"
		);

		static GUIContent scaleModeGuiContent = new GUIContent(
			"Scale Mode",
			"[Uniform] mode means thickness will also scale with the transform, regardless of thickness space settings\n\n" +
			"[Coordinate] mode means thickness values will remain the same even when scaling"
		);

		static GUIContent zTestGuiContent = new GUIContent(
			"Depth Test",
			"How this shape should render depending on the contents of the depth buffer. Note: anything other than [Less Equal] will not use GPU instancing\n\n" +
			"[Less Equal] means it will not render when behind something (default)\n\n" +
			"[Always] will completely ignore the depth buffer, drawing on top of everything else"
		);

		static GUIContent zOffsetFactorGuiContent = new GUIContent(
			"Depth Offset Factor",
			"Depth buffer offset factor, taking the slope into account (default is 0)\n\n" +
			"Practically, this is mostly used to be able to have two things on the same plane, but have one still render on top of the other without Z-fighting/flickering.\n" +
			"Setting this to, say, -1, will make this render on top of things in the same plane, setting this to 1 will make it render behind things on the same plane"
		);

		static GUIContent zOffsetUnitsGuiContent = new GUIContent(
			"Depth Offset Units",
			"Depth buffer offset, not taking the slope into account (default is 0)\n\n" +
			"I've never found much use of this one, seems like a bad version of Z offset factor? It's mostly here for completeness I guess"
		);

		public virtual void OnEnable() {
			soRnd = new SerializedObject( targets.Select( t => ( (Component)t ).GetComponent<MeshRenderer>() as Object ).ToArray() );
			propSortingOrder = soRnd.FindProperty( "m_SortingOrder" );
			propSortingLayer = soRnd.FindProperty( "m_SortingLayerID" );

			// will assign all null properties, even in derived types
			FindAllProperties();

			// hide mesh filter/renderer components
			foreach( ShapeRenderer shape in targets.Cast<ShapeRenderer>() )
				shape.HideMeshFilterRenderer();
		}

		void FindAllProperties() {
			IEnumerable<FieldInfo> GetFields( Type type ) {
				return type.GetFields( BindingFlags.Instance | BindingFlags.NonPublic )
					.Where( x => x.FieldType == typeof(SerializedProperty) && x.Name.StartsWith( "m_" ) == false && x.GetValue( this ) == null );
			}

			IEnumerable<FieldInfo> fieldsBase = GetFields( GetType().BaseType );
			IEnumerable<FieldInfo> fieldsInherited = GetFields( GetType() );

			foreach( FieldInfo field in fieldsBase.Concat( fieldsInherited ) ) {
				string fieldName = char.ToLowerInvariant( field.Name[4] ) + field.Name.Substring( 5 );
				field.SetValue( this, serializedObject.FindProperty( fieldName ) );
				if( field.GetValue( this ) == null )
					Debug.LogError( $"Failed to load {target.GetType()} property: {field.Name} !=> {fieldName}" );
			}
		}

		protected void BeginProperties( bool showColor = true ) {
			soRnd.Update();


			ShapesUI.BeginGroup();
			using( ShapesUI.Horizontal ) {
				using( ShapesUI.TempFieldWidth( 180f ) )
					ShapesUI.RenderSortingLayerField( propSortingLayer );
				using( ShapesUI.TempLabelWidth( 40f ) )
					EditorGUILayout.PropertyField( propSortingOrder, new GUIContent( "Order" ) );
			}

			EditorGUILayout.PropertyField( propZTest, zTestGuiContent );
			EditorGUILayout.PropertyField( propZOffsetFactor, zOffsetFactorGuiContent );
			EditorGUILayout.PropertyField( propZOffsetUnits, zOffsetUnitsGuiContent );

			// todo: add little warning box about instancing
			int uniqueCount = 0;
			int instancedCount = 0;
			foreach( ShapeRenderer obj in targets.Cast<ShapeRenderer>() ) {
				if( obj.IsUsingUniqueMaterials )
					uniqueCount++;
				else
					instancedCount++;
			}

			if( uniqueCount > 0 ) {
				string label;
				if( instancedCount == 0 ) {
					if( uniqueCount == 1 ) // single non-instanced
						label = "Note: this object is not GPU instanced due to custom depth settings";
					else // multiple exclusively non-instanced
						label = "Note: these objects are not GPU instanced due to custom depth settings";
				} else // mixed selection
					label = "Note: some of these objects are not GPU instanced due to custom depth settings";

				GUIStyle wrapLabel = new GUIStyle( EditorStyles.miniLabel );
				wrapLabel.wordWrap = true;
				using( ShapesUI.Horizontal ) {
					GUIContent icon = EditorGUIUtility.IconContent( "console.warnicon.sml" );
					GUILayout.Label( icon );
					GUILayout.TextArea( label, wrapLabel );
				}
			}

			ShapesUI.EndGroup();

			EditorGUILayout.PropertyField( propBlendMode, blendModeGuiContent );
			if( ( target as ShapeRenderer ).HasScaleModes )
				EditorGUILayout.PropertyField( propScaleMode, scaleModeGuiContent );
			if( showColor )
				PropertyFieldColor();
		}

		protected bool EndProperties() => soRnd.ApplyModifiedProperties() | serializedObject.ApplyModifiedProperties();

		protected void PropertyFieldColor() => EditorGUILayout.PropertyField( propColor );
		protected void PropertyFieldColor( string s ) => EditorGUILayout.PropertyField( propColor, new GUIContent( s ) );
		protected void PropertyFieldColor( GUIContent content ) => EditorGUILayout.PropertyField( propColor, content );

		public bool HasFrameBounds() => true;

		public Bounds OnGetFrameBounds() {
			if( serializedObject.isEditingMultipleObjects ) {
				// this only works for multiselecting shapes of the same type
				// todo: might be able to make a solution using Editor.CreateEditor shenanigans
				Bounds bounds = ( (ShapeRenderer)serializedObject.targetObjects[0] ).GetWorldBounds();
				for( int i = 1; i < serializedObject.targetObjects.Length; i++ )
					bounds.Encapsulate( ( (ShapeRenderer)serializedObject.targetObjects[i] ).GetWorldBounds() );
				return bounds;
			} else {
				return ( (ShapeRenderer)target ).GetWorldBounds();
			}
		}

	}

}