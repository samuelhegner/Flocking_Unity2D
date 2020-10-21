using UnityEngine;

namespace Shapes {

	public class Compass : MonoBehaviour {

		public Vector2 position;
		public float width = 1f;
		[Range( 0, 0.01f )] public float lineThickness = 0.1f;
		[Range( 0.1f, 2f )] public float bendRadius = 1f;
		[Range( 0.05f, ShapesMath.TAU * 0.49f )] public float fieldOfView = ShapesMath.TAU / 4;

		[Header( "Ticks" )] public int ticksPerQuarterTurn = 12;
		[Range( 0, 0.2f )] public float tickSize = 0.1f;
		[Range( 0f, 1f )] public float tickEdgeFadeFraction = 0.1f;
		[Range( 0.01f, 0.26f )] public float fontSizeTickLabel = 1f;
		[Range( 0, 0.1f )] public float tickLabelOffset = 0.01f;

		[Header( "Degree Marker" )] [Range( 0.01f, 0.26f )] public float fontSizeLookLabel = 1f;
		public Vector2 lookAngLabelOffset;
		[Range( 0, 0.05f )] public float triangleNootSize = 0.1f;

		public void DrawCompass( Vector3 worldDir ) {
			Vector2 compArcOrigin = position + Vector2.down * bendRadius;

			float angUiMin = ShapesMath.TAU * 0.25f - ( width / 2 ) / bendRadius;
			float angUiMax = ShapesMath.TAU * 0.25f + ( width / 2 ) / bendRadius;
			Vector2 dirWorld = new Vector2( worldDir.x, worldDir.z ).normalized;
			float lookAng = ShapesMath.DirToAng( dirWorld );
			float angWorldMin = lookAng + fieldOfView / 2;
			float angWorldMax = lookAng - fieldOfView / 2;

			Draw.Arc( compArcOrigin, bendRadius, lineThickness, angUiMin, angUiMax, ArcEndCap.Round );

			void CompassArcNoot( float worldAng, float size, string label = null ) {
				float tCompass = ShapesMath.InverseLerpAngleRad( angWorldMax, angWorldMin, worldAng );
				float uiAng = Mathf.Lerp( angUiMin, angUiMax, tCompass );
				Vector2 uiDir = ShapesMath.AngToDir( uiAng );
				Vector2 a = compArcOrigin + uiDir * bendRadius;
				Vector2 b = compArcOrigin + uiDir * ( bendRadius - size * tickSize );
				float fade = Mathf.InverseLerp( 0, tickEdgeFadeFraction, ( 1f - Mathf.Abs( tCompass * 2 - 1 ) ) );
				Draw.Line( a, b, LineEndCap.None, new Color( 1, 1, 1, fade ) );
				if( label != null ) {
					Draw.FontSize = fontSizeTickLabel;
					Draw.Text( b - uiDir * tickLabelOffset, uiAng - ShapesMath.TAU / 4f, label, TextAlign.Center, new Color( 1, 1, 1, fade ) );
				}
			}

			Draw.LineEndCaps = LineEndCap.Square;
			Draw.LineThickness = lineThickness;


			Vector2 trianglePos = compArcOrigin + Vector2.up * ( bendRadius + 0.01f );
			Vector2 labelPos = compArcOrigin + Vector2.up * ( bendRadius ) + lookAngLabelOffset * 0.1f;
			string lookLabel = Mathf.RoundToInt( -lookAng * Mathf.Rad2Deg + 180f ) + "°";
			Draw.FontSize = fontSizeLookLabel;
			Draw.Text( labelPos, 0f, lookLabel, TextAlign.Center );
			Vector2 triA = trianglePos + ShapesMath.AngToDir( -ShapesMath.TAU / 4 ) * triangleNootSize;
			Vector2 triB = trianglePos + ShapesMath.AngToDir( -ShapesMath.TAU / 4 + ShapesMath.TAU / 3 ) * triangleNootSize;
			Vector2 triC = trianglePos + ShapesMath.AngToDir( -ShapesMath.TAU / 4 + 2 * ShapesMath.TAU / 3 ) * triangleNootSize;
			Draw.Triangle( triA, triB, triC );

			int tickCount = ( ticksPerQuarterTurn - 1 ) * 4;
			for( int i = 0; i < tickCount; i++ ) {
				float t = i / ( (float)tickCount );
				float ang = ShapesMath.TAU * t;
				bool cardinal = i % ( tickCount / 4 ) == 0;

				string label = null;
				if( cardinal ) {
					int angInt = Mathf.RoundToInt( ( 1f - t ) * 4 );
					switch( angInt ) {
						case 0:
						case 4:
							label = "S";
							break;
						case 1:
							label = "W";
							break;
						case 2:
							label = "N";
							break;
						case 3:
							label = "E";
							break;
					}
				}

				float tCompass = ShapesMath.InverseLerpAngleRad( angWorldMax, angWorldMin, ang );
				if( tCompass < 1f && tCompass > 0f )
					CompassArcNoot( ang, cardinal ? 0.8f : 0.5f, label );
			}
		}

	}

}