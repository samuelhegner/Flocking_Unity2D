using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class AboutWindow : EditorWindow {

		class Center : IDisposable {
			public Center() => Noot();
			public void Dispose() => Noot();
			static void Noot() => GUILayout.Label( GUIContent.none, GUILayout.ExpandWidth( true ) );
		}

		class CenterVertical : IDisposable {
			public CenterVertical() => Noot();
			public void Dispose() => Noot();
			static void Noot() => GUILayout.Label( GUIContent.none, GUILayout.ExpandHeight( true ) );
		}

		class Doot {
			public Doot( float angOffset, float angSpeed, float radialOffset ) {
				this.angOffset = angOffset;
				this.angSpeed = angSpeed;
				this.radialOffset = radialOffset;
			}

			public float angOffset;
			public float angSpeed;
			public float radialOffset;
			public Vector2 pos;

		}

		const float DOOT_MAX_RADIUS = 0.95f;
		static Doot[] doots;
		static Color colMain = new Color( 1, 1, 1, 1f );
		static Color colFade = new Color( 0f, 0.1f, 0.1f, 0f );
		static Color colF15 = new Color( 1f, 0.0666666f, 0.333333f, 1f );

		GUIStyle labelStyle;
		GUIStyle LabelStyle => labelStyle ?? ( labelStyle = new GUIStyle( GUI.skin.label ) { active = { textColor = Color.white }, normal = { textColor = Color.white } } );
		GUIStyle linkStyle;
		GUIStyle LinkStyle => linkStyle ?? ( linkStyle = new GUIStyle( LabelStyle ) { hover = { textColor = colF15 } } );
		GUIStyle titleStyle;
		GUIStyle TitleStyle => titleStyle ?? ( titleStyle = new GUIStyle( EditorStyles.boldLabel ) { fontSize = 26, alignment = TextAnchor.MiddleCenter, active = { textColor = Color.white }, normal = { textColor = Color.white } } );
		GUIStyle labelCentered;
		GUIStyle LabelCentered => labelCentered ?? ( labelCentered = new GUIStyle( GUI.skin.label ) { alignment = TextAnchor.MiddleCenter, active = { textColor = Color.white }, normal = { textColor = Color.white } } );

		string newVersionAvailable = null;
		UnityWebRequest req;

		bool WebRequestHasErrors {
			get {
				#if UNITY_2020_1_OR_NEWER
				return req.result == UnityWebRequest.Result.ProtocolError || req.result == UnityWebRequest.Result.ConnectionError;
				#else
				return req.isHttpError || req.isNetworkError;
				#endif
			}
		}

		void OnEnable() {
			req = UnityWebRequest.Get( ShapesInfo.LINK_LATEST_VERSION );
			req.SendWebRequest();

			const int count = 16;
			doots = new Doot[count];
			for( int i = 0; i < count; i++ ) {
				const float maxSpeed = 0.3f;
				const float minSpeed = 0.1f;
				float speed = Random.Range( minSpeed, maxSpeed ) * Mathf.Sign( Random.value - 0.5f );
				doots[i] = new Doot( Random.value * ShapesMath.TAU, speed, Random.Range( 0.5f, DOOT_MAX_RADIUS ) );
			}

			EditorApplication.update += Update;
		}

		void OnDisable() => EditorApplication.update -= Update;

		void Update() {
			if( req != null ) {
				if( req.isDone ) {
					OnReceiveLatestVersion( req.downloadHandler.text );
					req.Dispose();
					req = null;
				} else if( WebRequestHasErrors ) {
					Debug.Log( req.error );
					req.Dispose();
					req = null;
				}
			}

			Repaint();
		}

		void OnReceiveLatestVersion( string latestVersion ) {
			int[] GetVersionNums( string str ) => str.Split( '.' ).Select( int.Parse ).ToArray();
			int[] latest = GetVersionNums( latestVersion );
			int[] current = GetVersionNums( ShapesInfo.Version );
			newVersionAvailable = null;
			for( int i = 0; i < 3; i++ ) {
				if( current[i] > latest[i] )
					return; // local version is newer somehow. you're probably freya. or a hacker >:I
				if( latest[i] > current[i] ) {
					newVersionAvailable = latestVersion;
					return;
				}
			}
		}


		void OnGUI() {
			if( Event.current.type == EventType.Repaint )
				DrawShapes();
			DrawText();
			if( Event.current.type == EventType.MouseDown ) {
				GUI.FocusControl( null );
				Repaint();
			}
		}

		void DrawText() {
			using( new CenterVertical() ) {
				GUI.color = colMain;

				if( newVersionAvailable != null ) {
					using( ShapesUI.Horizontal ) {
						using( new Center() ) {
							float t = (float)EditorApplication.timeSinceStartup;
							float wave = ShapesMath.SmoothCos01( Mathf.PingPong( t, 0.5f ) * 2 );
							wave = Mathf.Lerp( 0.5f, 1f, wave );
							GUI.color = Color.Lerp( Color.white, colF15, wave );
							LinkLabel( newVersionAvailable + " now available", ShapesInfo.LINK_CHANGELOG );
							GUI.color = Color.white;
						}
					}
				}


				GUILayout.Label( $"Shapes {ShapesInfo.Version}", TitleStyle );
				using( ShapesUI.Horizontal ) {
					using( new Center() ) {
						int year = Mathf.Max( DateTime.Now.Year, 2020 ); // just in case your computer clock is wonky~
						GUILayout.Label( $"© {year}", LabelStyle, GUILayout.ExpandWidth( false ) );
						LinkLabel( "Freya Holmér", ShapesInfo.LINK_TWITTER );
					}
				}

				GUI.color = Color.white;
				GUILayout.Space( 8 );
				GUILayout.Label( "made possible thanks to\nthe wonderful supporters on", LabelCentered, GUILayout.ExpandWidth( true ) );
				using( ShapesUI.Horizontal ) {
					using( new Center() ) {
						LinkLabel( "Patreon", ShapesInfo.LINK_PATREON );
					}
				}

				GUI.color = colF15;
				GUILayout.Label( "♥", TitleStyle, GUILayout.ExpandWidth( true ) );
				GUI.color = Color.white;
			}
		}

		float mouseDootT = 0f;

		void DrawShapes() {
			Vector2 center = position.size / 2;
			float fitRadius = Mathf.Min( position.width, position.height ) / 2 - 8;

			// set doot positions
			float t = (float)EditorApplication.timeSinceStartup / 2;
			foreach( Doot doot in doots ) {
				float ang = doot.angSpeed * t * ShapesMath.TAU + doot.angOffset;
				Vector2 dir = ShapesMath.AngToDir( ang );
				doot.pos = dir * ( fitRadius * doot.radialOffset );
			}

			// mouse doot~
			Vector2 mouseRawPos = Event.current.mousePosition - center;
			float maxRadius = fitRadius * DOOT_MAX_RADIUS;
			Vector2 mouseTargetPos = Vector2.ClampMagnitude( mouseRawPos, maxRadius );
			doots[0].pos = Vector2.Lerp( doots[0].pos, mouseTargetPos, mouseDootT );
			bool mouseOver = mouseOverWindow == this;
			mouseDootT = Mathf.Lerp( mouseDootT, mouseOver ? 1f : 0f, 0.05f );


			// save state
			Matrix4x4 prevMtx = Draw.Matrix;
			ShapesBlendMode prevBlendMode = Draw.BlendMode;
			ThicknessSpace prevDiscRadiusSpace = Draw.DiscRadiusSpace;
			ThicknessSpace prevLineThicknessSpace = Draw.LineThicknessSpace;
			LineGeometry prevLineGeometry = Draw.LineGeometry;
			ThicknessSpace prevRingThicknessSpace = Draw.RingThicknessSpace;

			// draw setup
			Draw.Matrix = Matrix4x4.TRS( new Vector3( center.x, center.y, 1f ), Quaternion.identity, Vector3.one );
			Draw.BlendMode = ShapesBlendMode.Transparent;
			Draw.DiscRadiusSpace = ThicknessSpace.Meters;
			Draw.LineThicknessSpace = ThicknessSpace.Meters;
			Draw.LineGeometry = LineGeometry.Flat2D;
			Draw.RingThicknessSpace = ThicknessSpace.Meters;

			// Drawing
			Draw.RingGradientRadial( Vector3.zero, fitRadius, fitRadius * 0.1f, Color.black, new Color( 0, 0, 0, 0 ) );
			Draw.Disc( Vector3.zero, fitRadius, Color.black );

			// edge noodles
			const int noodCount = 64;
			for( int i = 0; i < noodCount; i++ ) {
				float tDir = i / ( (float)noodCount );
				float tAng = ShapesMath.TAU * tDir;
				Vector2 dir = ShapesMath.AngToDir( tAng );
				if( Mathf.Abs( dir.y ) > 0.75f )
					continue;
				Vector2 root = dir * fitRadius;
				float distToNearest = float.MaxValue;
				for( int j = 0; j < doots.Length; j++ )
					distToNearest = Mathf.Min( distToNearest, Vector2.Distance( doots[j].pos, root ) );
				float distMod = Mathf.InverseLerp( fitRadius * 0.5f, fitRadius * 0.1f, distToNearest );
				float noodMaxOffset = fitRadius * ( 1 + 0.1f * distMod );
				Draw.Line( root, dir * noodMaxOffset, fitRadius * Mathf.Lerp( 0.07f, 0.04f, distMod ) );
			}

			// ring
			Draw.Ring( Vector3.zero, fitRadius, fitRadius * 0.0125f, colMain );

			// connecting lines
			for( int i = 0; i < doots.Length; i++ ) {
				Vector2 a = doots[i].pos;
				for( int j = i; j < doots.Length; j++ ) {
					Vector2 b = doots[j].pos;
					float dist = Vector2.Distance( a, b );
					float rangeValue = Mathf.InverseLerp( fitRadius * 1f, fitRadius * 0.02f, dist );
					if( rangeValue > 0 ) {
						Color col = Color.Lerp( colFade, colMain, rangeValue );
						Draw.Line( a, b, fitRadius * 0.015f * rangeValue, LineEndCap.Round, col );
					}
				}
			}

			// doots~
			foreach( Doot doot in doots ) {
				Draw.BlendMode = ShapesBlendMode.Transparent;
				Draw.Disc( doot.pos, fitRadius * 0.025f, Color.black );
				Draw.Disc( doot.pos, fitRadius * 0.015f, colMain );
				Draw.BlendMode = ShapesBlendMode.Additive;
				Color innerColor = colMain;
				innerColor.a = 0.25f;
				Color outerColor = Color.clear;
				Draw.DiscGradientRadial( doot.pos, fitRadius * 0.18f, innerColor, outerColor );
			}

			Draw.BlendMode = ShapesBlendMode.Multiplicative;
			Draw.DiscGradientRadial( Vector3.zero, fitRadius * 0.5f, Color.black, Color.clear );


			// restore state
			Draw.Matrix = prevMtx;
			Draw.BlendMode = prevBlendMode;
			Draw.DiscRadiusSpace = prevDiscRadiusSpace;
			Draw.LineThicknessSpace = prevLineThicknessSpace;
			Draw.LineGeometry = prevLineGeometry;
			Draw.RingThicknessSpace = prevRingThicknessSpace;
		}


		void LinkLabel( string label, string url ) {
			GUIContent content = new GUIContent( label );
			if( GUILayout.Button( content, LinkStyle, GUILayout.ExpandWidth( false ) ) )
				Application.OpenURL( url );
			Rect r = GUILayoutUtility.GetLastRect();
			EditorGUIUtility.AddCursorRect( r, MouseCursor.Link );
			Handles.BeginGUI();
			Color c = GUI.color;
			c.a = 0.5f;
			Handles.color = c;
			float y = r.yMax - 1;
			float margin = 2;
			Handles.DrawLine( new Vector3( r.xMin + margin, y ), new Vector3( r.xMax - margin, y ) );
			Handles.color = Color.white;
			Handles.EndGUI();
		}


	}

}