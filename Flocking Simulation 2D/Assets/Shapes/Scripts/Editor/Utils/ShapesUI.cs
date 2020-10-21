using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesUI {

		public static GUIStyle GetMiniButtonStyle( int i, int maxCount ) {
			if( maxCount > 1 ) {
				if( i == 0 )
					return EditorStyles.miniButtonLeft;
				if( i == maxCount - 1 )
					return EditorStyles.miniButtonRight;
				return EditorStyles.miniButtonMid;
			}

			return EditorStyles.miniButton;
		}

		public static int EnumButtonRow( int currentVal, string[] labels, bool hiddenZeroValue ) {
			int iOffset = hiddenZeroValue ? 1 : 0;
			int count = labels.Length;
			int returnVal = currentVal;

			GUILayout.BeginHorizontal();
			for( int i = iOffset; i < count; i++ ) {
				GUIStyle style = GetMiniButtonStyle( i - iOffset, count - iOffset );
				bool pressedBefore = i == currentVal;
				bool pressedAfter = GUILayout.Toggle( pressedBefore, labels[i], style );
				if( pressedBefore == false && pressedAfter == true ) {
					returnVal = i;
				}

				if( hiddenZeroValue && pressedBefore == true && pressedAfter == false ) {
					returnVal = 0;
				}
			}

			GUILayout.EndHorizontal();

			return returnVal;
		}

		public static void RepaintAllSceneViews() {
			foreach( SceneView sceneView in SceneView.sceneViews )
				sceneView.Repaint();
		}

		static Assembly editorAssembly;
		static Assembly EditorAssembly {
			get {
				if( editorAssembly == null ) {
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					editorAssembly = assemblies.First( x => x.FullName.StartsWith( "UnityEditor," ) );
				}

				return editorAssembly;
			}
		}


		public static void ShowColorPicker( Action<Color> colorChangedCallback, Color col ) {
			Type colorPickerType = EditorAssembly.GetType( "UnityEditor.ColorPicker", throwOnError: true );
			MethodInfo showColorPicker = colorPickerType.GetMethod( "Show", BindingFlags.Static | BindingFlags.Public, Type.DefaultBinder, CallingConventions.Any, new Type[] { typeof(Action<Color>), typeof(Color), typeof(bool), typeof(bool) }, null );
			showColorPicker.Invoke( null, new object[] { colorChangedCallback, col, true, ShapesConfig.USE_HDR_COLOR_PICKERS } );
		}


		public static bool DrawTypeSwitchButtons( SerializedProperty prop, GUIContent[] guiContent, int[] indexOverride = null, int height = 20 ) {

			int GetEntryCount() {
				switch( prop.propertyType ) {
					case SerializedPropertyType.Enum:    return prop.enumNames.Length;
					case SerializedPropertyType.Integer: return indexOverride.Length;
					default:                             throw new IndexOutOfRangeException( "no, illegal >:I" );
				}
			}

			bool[] multiselectPressedState = prop.TryGetMultiselectPressedStates( indexOverride, GetEntryCount() );

			bool changed = false;
			SerializedObject so = prop.serializedObject;
			bool multiselect = so.isEditingMultipleObjects;

			EditorGUI.BeginChangeCheck();
			GUILayoutOption[] buttonLayout = { GUILayout.Height( height ), GUILayout.MinWidth( height ) };

			void EnumButton( int index ) {
				GUIStyle style = ShapesUI.GetMiniButtonStyle( index, guiContent.Length );
				bool btnState;
				if( multiselect )
					btnState = multiselectPressedState[index];
				else
					btnState = index == prop.GetIntValue( indexOverride ) && prop.hasMultipleDifferentValues == false;
				bool btnStateNew = GUILayout.Toggle( btnState, guiContent[index], style, buttonLayout );

				bool pressedInMultiselect = multiselect && btnState != btnStateNew;
				bool pressedInSingleselect = multiselect == false && btnStateNew && btnState == false;

				if( pressedInMultiselect || pressedInSingleselect ) {
					prop.SetIntValue( index, indexOverride );
					changed = true;
				}
			}

			GUILayout.BeginHorizontal();
			for( int i = 0; i < guiContent.Length; i++ )
				EnumButton( i );
			GUILayout.EndHorizontal();
			return changed;
		}


		public static void AngleProperty( SerializedProperty prop, string label, SerializedProperty unitProp, params GUILayoutOption[] layout ) {
			AngularUnit unit = unitProp.hasMultipleDifferentValues ? AngularUnit.Radians : (AngularUnit)unitProp.enumValueIndex;

			using( Horizontal ) {
				// value field
				using( EditorGUI.ChangeCheckScope chChk = new EditorGUI.ChangeCheckScope() ) {
					EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
					float newValue = EditorGUILayout.FloatField( label, prop.floatValue * unit.FromRadians() ) * unit.ToRadians();
					if( chChk.changed )
						prop.floatValue = newValue;
					EditorGUI.showMixedValue = false;
				}

				// unit suffix
				GUILayout.Label( unit.Suffix(), layout );
			}
		}

		public static void DrawAngleSwitchButtons( SerializedProperty prop ) {
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel( " " );
			GUIContent[] angLabels = ( Screen.width < 300 ) ? ShapesAssets.AngleUnitButtonContentsShort : ShapesAssets.AngleUnitButtonContents;
			ShapesUI.DrawTypeSwitchButtons( prop, angLabels, null, 15 );
			GUILayout.EndHorizontal();
		}

		public static void EnumToggleProperty( SerializedProperty enumProp, string label ) {
			using( var chChk = new EditorGUI.ChangeCheckScope() ) {
				EditorGUI.showMixedValue = enumProp.hasMultipleDifferentValues;
				bool newValue = EditorGUILayout.Toggle( new GUIContent( label ), enumProp.enumValueIndex == 1 );
				if( chChk.changed )
					enumProp.enumValueIndex = newValue.AsInt();
				EditorGUI.showMixedValue = false;
			}
		}

		public static void BeginGroup() => GUILayout.BeginVertical( EditorStyles.helpBox );
		public static void EndGroup() => GUILayout.EndVertical();

		public static void PropertyLabelWidth( SerializedProperty prop, string label, float labelWidth, params GUILayoutOption[] options ) {
			using( TempLabelWidth( labelWidth ) )
				EditorGUILayout.PropertyField( prop, new GUIContent( label ), options );
		}

		public static void PropertyFieldWidth( SerializedProperty prop, string label, float fieldWidth, params GUILayoutOption[] options ) {
			using( TempFieldWidth( fieldWidth ) )
				EditorGUILayout.PropertyField( prop, label == null ? GUIContent.none : new GUIContent( label ), options );
		}

		struct TemporaryColor : IDisposable {
			static readonly Stack<Color> temporaryColors = new Stack<Color>();

			public TemporaryColor( Color color ) {
				temporaryColors.Push( GUI.color );
				GUI.color = color;
			}

			public void Dispose() => GUI.color = temporaryColors.Pop();
		}

		public static TemporaryLabelWidth TempLabelWidth( float width ) => new TemporaryLabelWidth( width );

		public struct TemporaryLabelWidth : IDisposable {
			static readonly Stack<float> temporaryWidths = new Stack<float>();

			public TemporaryLabelWidth( float width ) {
				temporaryWidths.Push( EditorGUIUtility.labelWidth );
				EditorGUIUtility.labelWidth = width;
			}

			public void Dispose() => EditorGUIUtility.labelWidth = temporaryWidths.Pop();
		}

		public static TemporaryFieldWidth TempFieldWidth( float width ) => new TemporaryFieldWidth( width );

		public struct TemporaryFieldWidth : IDisposable {
			static readonly Stack<float> temporaryWidths = new Stack<float>();

			public TemporaryFieldWidth( float width ) {
				temporaryWidths.Push( EditorGUIUtility.fieldWidth );
				EditorGUIUtility.fieldWidth = width;
			}

			public void Dispose() => EditorGUIUtility.fieldWidth = temporaryWidths.Pop();
		}


		public static void FloatInSpaceField( SerializedProperty value, SerializedProperty space, bool spaceEnabled = true ) {
			using( Horizontal ) {
				EditorGUILayout.PropertyField( value );
				using( EnabledIf( spaceEnabled ) )
					EditorGUILayout.PropertyField( space, GUIContent.none, GUILayout.Width( 64 ) );
			}
		}

		public const int POS_COLOR_FIELD_LABEL_WIDTH = 32;
		public const int POS_COLOR_FIELD_COLOR_WIDTH = 50;
		public const int POS_COLOR_FIELD_THICKNESS_WIDTH = 52;

		static TemporaryColor TempColor( Color color ) => new TemporaryColor( color );
		public static EditorGUILayout.HorizontalScope Horizontal => new EditorGUILayout.HorizontalScope();
		public static EditorGUILayout.VerticalScope Vertical => new EditorGUILayout.VerticalScope();
		static EditorGUI.DisabledScope EnabledIf( bool enabled ) => new EditorGUI.DisabledScope( enabled == false );

		public static void PosColorField( string label, SerializedProperty pos, SerializedProperty col, bool colorEnabled = true, bool positionEnabled = true ) {
			PosColorField( label, () => EditorGUILayout.PropertyField( pos, GUIContent.none ), col, colorEnabled, positionEnabled );
		}

		public static void PosColorFieldPosOff( string label, Vector3 displayPos, SerializedProperty col, bool colorEnabled = true ) {
			PosColorField( label, () => EditorGUILayout.Vector3Field( GUIContent.none, displayPos, GUILayout.MinWidth( 32f ) ), col, colorEnabled, false );
		}

		public static void PosColorFieldSpecialOffState( string label, SerializedProperty pos, Vector3 offDisplayPos, SerializedProperty col, bool colorEnabled = true, bool positionEnabled = true ) {
			if( positionEnabled )
				PosColorField( label, pos, col, colorEnabled );
			else
				PosColorFieldPosOff( label, offDisplayPos, col, colorEnabled );
		}

		public static void PosColorField( Rect rect, SerializedProperty pos, SerializedProperty col, bool colorEnabled = true ) {
			Rect rectColor = rect;
			rectColor.xMin = rect.xMax - POS_COLOR_FIELD_COLOR_WIDTH;
			rectColor.xMax = rect.xMax;
			Rect rectPos = rect;
			rectPos.width -= rectColor.width;
			EditorGUI.PropertyField( rectPos, pos, GUIContent.none );
			using( EnabledIf( colorEnabled ) )
				using( TempColor( colorEnabled ? Color.white : Color.clear ) )
					EditorGUI.PropertyField( rectColor, col, GUIContent.none );
		}

		public static void PosThicknessColorField( Rect rect, SerializedProperty pos, SerializedProperty thickness, SerializedProperty col, bool colorEnabled = true ) {
			const float THICKNESS_MARGIN = 2;
			const float rightSideWidth = POS_COLOR_FIELD_COLOR_WIDTH + POS_COLOR_FIELD_THICKNESS_WIDTH + THICKNESS_MARGIN;

			Rect rectColor = rect;
			rectColor.x = rect.xMax - POS_COLOR_FIELD_COLOR_WIDTH;
			rectColor.width = POS_COLOR_FIELD_COLOR_WIDTH;

			Rect rectThickness = rect;
			rectThickness.x = rect.xMax - rightSideWidth + THICKNESS_MARGIN;
			rectThickness.width = POS_COLOR_FIELD_THICKNESS_WIDTH;

			Rect rectPos = rect;
			rectPos.width -= rightSideWidth;

			EditorGUI.PropertyField( rectPos, pos, GUIContent.none );
			using( TempLabelWidth( 18 ) )
				EditorGUI.PropertyField( rectThickness, thickness, new GUIContent( "Th", "thickness" ) );
			using( EnabledIf( colorEnabled ) )
				using( TempColor( colorEnabled ? Color.white : Color.clear ) )
					EditorGUI.PropertyField( rectColor, col, GUIContent.none );
		}

		static void PosColorField( string label, Action field, SerializedProperty col, bool colorEnabled = true, bool positionEnabled = true ) {
			using( Horizontal ) {
				GUILayout.Label( label, GUILayout.Width( POS_COLOR_FIELD_LABEL_WIDTH ) );
				using( EnabledIf( positionEnabled ) )
					field();
				using( EnabledIf( colorEnabled ) )
					using( TempColor( colorEnabled ? Color.white : Color.clear ) )
						EditorGUILayout.PropertyField( col, GUIContent.none, GUILayout.Width( POS_COLOR_FIELD_COLOR_WIDTH ) );
			}
		}


		// used for sorting layer popup
		static GUIStyle boldPopupStyle;
		static GUIStyle BoldPopupStyle => boldPopupStyle ?? ( boldPopupStyle = new GUIStyle( EditorStyles.popup ) { fontStyle = FontStyle.Bold } );
		static GUIContent sortingLayerStyle = EditorGUIUtility.TrTextContent( "Sorting Layer", "Name of the Renderer's sorting layer" );

		static MethodInfo sortingLayerField;
		static MethodInfo SortingLayerField {
			get {
				if( sortingLayerField == null )
					sortingLayerField = typeof(EditorGUILayout).GetMethod(
						"SortingLayerField",
						BindingFlags.Static | BindingFlags.NonPublic,
						Type.DefaultBinder,
						new Type[] { typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle), typeof(GUIStyle) },
						null
					);
				return sortingLayerField;
			}
		}

		public static void RenderSortingLayerField( SerializedProperty sortingLayer ) {
			bool hasPrefabOverride = sortingLayer.HasPrefabOverride();
			SortingLayerField.Invoke( null, new object[] {
				sortingLayerStyle,
				sortingLayer,
				hasPrefabOverride ? BoldPopupStyle : EditorStyles.popup,
				hasPrefabOverride ? EditorStyles.boldLabel : EditorStyles.label
			} );
		}


		static MethodInfo setIconEnabled; // haha long line go brrrr
		static MethodInfo SetIconEnabled => setIconEnabled = setIconEnabled ?? Assembly.GetAssembly( typeof(Editor) )?.GetType( "UnityEditor.AnnotationUtility" )?.GetMethod( "SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic );

		public static void SetGizmoIconEnabled( Type type, bool on ) {
			if( SetIconEnabled == null ) return;
			const int MONO_BEHAVIOR_CLASS_ID = 114; // https://docs.unity3d.com/Manual/ClassIDReference.html
			SetIconEnabled.Invoke( null, new object[] { MONO_BEHAVIOR_CLASS_ID, type.Name, on ? 1 : 0 } );
		}

	}

}