using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class PolylinePath : DisposableMesh {

		List<PolylinePoint> path = new List<PolylinePoint>();

		bool lastUsedClosed = false;
		PolylineJoins lastUsedJoins = PolylineJoins.Miter;

		public int Count => path.Count;
		public PolylinePoint LastPoint => path[path.Count - 1];

		public PolylinePath() => _ = 0;

		#region accessors and setters by index

		public PolylinePoint this[ int i ] {
			get => path[i];
			set {
				path[i] = value;
				meshDirty = true;
			}
		}

		public void SetPoint( int index, Vector3 point ) {
			PolylinePoint p = path[index];
			p.point = point;
			SetPoint( index, p );
		}

		public void SetPoint( int index, Vector2 point ) {
			PolylinePoint p = path[index];
			p.point = point;
			SetPoint( index, p );
		}

		public void SetColor( int index, Color color ) {
			PolylinePoint p = path[index];
			p.color = color;
			SetPoint( index, p );
		}

		public void SetPoint( int index, PolylinePoint point ) {
			path[index] = point;
			meshDirty = true;
		}

		#endregion

		#region point adding

		public void AddPoint( float x, float y ) => AddPoint( new PolylinePoint( new Vector3( x, y, 0f ), Color.white ) );
		public void AddPoint( float x, float y, float z ) => AddPoint( new PolylinePoint( new Vector3( x, y, z ), Color.white ) );
		public void AddPoint( float x, float y, Color color ) => AddPoint( new PolylinePoint( new Vector3( x, y, 0f ), color ) );
		public void AddPoint( float x, float y, float z, Color color ) => AddPoint( new PolylinePoint( new Vector3( x, y, z ), color ) );
		public void AddPoint( Vector3 pos ) => AddPoint( new PolylinePoint( pos, Color.white ) );
		public void AddPoint( Vector3 pos, Color color ) => AddPoint( new PolylinePoint( pos, color ) );
		public void AddPoint( Vector3 pos, float thickness ) => AddPoint( new PolylinePoint( pos, Color.white, thickness ) );
		public void AddPoint( Vector3 pos, float thickness, Color color ) => AddPoint( new PolylinePoint( pos, color, thickness ) );
		public void AddPoint( Vector2 pos ) => AddPoint( new PolylinePoint( pos, Color.white ) );
		public void AddPoint( Vector2 pos, Color color ) => AddPoint( new PolylinePoint( pos, color ) );
		public void AddPoint( Vector2 pos, float thickness ) => AddPoint( new PolylinePoint( pos, Color.white, thickness ) );
		public void AddPoint( Vector2 pos, float thickness, Color color ) => AddPoint( new PolylinePoint( pos, color, thickness ) );


		public void AddPoint( PolylinePoint p ) {
			if( hasSetFirstPoint == false )
				OnSetFirstDataPoint();
			path.Add( p );
			meshDirty = true;
		}

		public void AddPoints( IEnumerable<Vector3> pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		public void AddPoints( params Vector3[] pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		public void AddPoints( IEnumerable<Vector2> pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		public void AddPoints( params Vector2[] pts ) => AddPoints( pts.Select( point => new PolylinePoint( point, Color.white ) ) );
		public void AddPoints( IEnumerable<Vector3> pts, Color color ) => AddPoints( pts.Select( point => new PolylinePoint( point, color ) ) );
		public void AddPoints( IEnumerable<Vector2> pts, Color color ) => AddPoints( pts.Select( point => new PolylinePoint( point, color ) ) );
		public void AddPoints( IEnumerable<Vector3> pts, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
		public void AddPoints( IEnumerable<Vector2> pts, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
		public void AddPoints( IEnumerable<Vector3> pts, IEnumerable<float> thicknesses ) => AddPoints( pts.Zip( thicknesses, ( p, t ) => new PolylinePoint( p, Color.white, t ) ) );
		public void AddPoints( IEnumerable<Vector2> pts, IEnumerable<float> thicknesses ) => AddPoints( pts.Zip( thicknesses, ( p, t ) => new PolylinePoint( p, Color.white, t ) ) );
		public void AddPoints( IEnumerable<Vector3> pts, IEnumerable<float> thicknesses, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, thicknesses, ( p, c, t ) => new PolylinePoint( p, c, t ) ) );
		public void AddPoints( IEnumerable<Vector2> pts, IEnumerable<float> thicknesses, IEnumerable<Color> colors ) => AddPoints( pts.Zip( colors, thicknesses, ( p, c, t ) => new PolylinePoint( p, c, t ) ) );

		public void AddPoints( params PolylinePoint[] pts ) => AddPoints( (IEnumerable<PolylinePoint>)pts );

		public void AddPoints( IEnumerable<PolylinePoint> ptsToAdd ) {
			int prevCount = path.Count;
			path.AddRange( ptsToAdd );
			int pathCount = path.Count;
			int addedPtCount = pathCount - prevCount;

			if( addedPtCount > 0 ) {
				if( hasSetFirstPoint == false )
					OnSetFirstDataPoint();
			}
		}

		#endregion

		#region BezierTo, ArcTo

		bool CheckInvalidToCall( [CallerMemberName] string callerName = null ) {
			if( hasSetFirstPoint == false ) {
				Debug.LogWarning( $"{callerName} requires adding a point before calling it, to determine starting point" );
				return true;
			}

			return false;
		}

		/// <summary>
		/// Adds points of a cubic bezier curve, using the previous point as the starting point
		/// </summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, int pointCount ) => BezierTo( startTangent, endTangent, end, pointCount, Color.white );

		/// <summary>
		/// Adds points of a cubic bezier curve, using the previous point as the starting point
		/// </summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, int pointCount, Color color ) {
			if( CheckInvalidToCall() ) return;
			AddPoints( ShapesMath.CubicBezierPointsSkipFirst( LastPoint.point, startTangent, endTangent, end, pointCount ), color );
		}

		/// <summary>
		/// A cubic bezier curve, using the previous point as the starting point
		/// </summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end ) => BezierTo( startTangent, endTangent, end, ShapesConfig.POLYLINE_DEFAULT_POINTS_PER_TURN, Color.white );

		/// <summary>
		/// A cubic bezier curve, using the previous point as the starting point
		/// </summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, Color color ) => BezierTo( startTangent, endTangent, end, ShapesConfig.POLYLINE_DEFAULT_POINTS_PER_TURN, color );

		/// <summary>
		/// A cubic bezier curve, using the previous point as the starting point. Number of points is given by density in number of points per full 360° turn
		/// </summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, float pointsPerTurn ) => BezierTo( startTangent, endTangent, end, pointsPerTurn, Color.white );

		/// <summary>
		/// A cubic bezier curve, using the previous point as the starting point. Number of points is given by density in number of points per full 360° turn
		/// </summary>
		public void BezierTo( Vector3 startTangent, Vector3 endTangent, Vector3 end, float pointsPerTurn, Color color ) {
			int sampleCount = ShapesConfig.POLYLINE_BEZIER_ANGULAR_SUM_ACCURACY * 2 + 1;
			float curveSumDeg = ShapesMath.GetApproximateCurveSum( LastPoint.point, startTangent, endTangent, end, sampleCount );
			float angSpanTurns = curveSumDeg / 360f;
			int pointCount = Mathf.Max( 2, Mathf.RoundToInt( angSpanTurns * ShapesConfig.POLYLINE_DEFAULT_POINTS_PER_TURN ) );
			BezierTo( startTangent, endTangent, end, pointCount, color );
		}

		/// <summary>
		/// Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point count
		/// </summary>
		public void ArcTo( Vector3 corner, Vector3 next, float radius, int pointCount ) {
			if( CheckInvalidToCall() ) return;
			AddArcPoints( corner, next, radius, useDensity: false, pointCount, 0, Color.white );
		}

		/// <summary>
		/// Adds points of an arc wedged into the corner defined by the previous point, corner, and next
		/// </summary>
		public void ArcTo( Vector3 corner, Vector3 next, float radius ) {
			if( CheckInvalidToCall() ) return;
			AddArcPoints( corner, next, radius, useDensity: true, 0, ShapesConfig.POLYLINE_DEFAULT_POINTS_PER_TURN, Color.white );
		}

		/// <summary>
		/// Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point density in number of points per full 360° turn
		/// </summary>
		public void ArcTo( Vector3 corner, Vector3 next, float radius, float pointsPerTurn ) {
			if( CheckInvalidToCall() ) return;
			AddArcPoints( corner, next, radius, useDensity: true, 0, pointsPerTurn, Color.white );
		}

		/// <summary>
		/// Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point count
		/// </summary>
		public void ArcTo( Vector3 corner, Vector3 next, float radius, int pointCount, Color color ) {
			if( CheckInvalidToCall() ) return;
			AddArcPoints( corner, next, radius, useDensity: false, pointCount, 0, color );
		}

		/// <summary>
		/// Adds points of an arc wedged into the corner defined by the previous point, corner, and next
		/// </summary>
		public void ArcTo( Vector3 corner, Vector3 next, float radius, Color color ) {
			if( CheckInvalidToCall() ) return;
			AddArcPoints( corner, next, radius, useDensity: true, 0, ShapesConfig.POLYLINE_DEFAULT_POINTS_PER_TURN, color );
		}

		/// <summary>
		/// Adds points of an arc wedged into the corner defined by the previous point, corner, and next, with the given point density in number of points per full 360° turn
		/// </summary>
		public void ArcTo( Vector3 corner, Vector3 next, float radius, float pointsPerTurn, Color color ) {
			if( CheckInvalidToCall() ) return;
			AddArcPoints( corner, next, radius, useDensity: true, 0, pointsPerTurn, color );
		}

		void AddArcPoints( Vector3 corner, Vector3 next, float radius, bool useDensity, int targetPointCount, float pointsPerTurn, Color color ) {
			if( radius <= 0.0001f ) {
				// radius is super small, just add the corner point
				AddPoint( corner, color );
				return; // pretty much just a straight line. only add the corner point
			}

			Vector3 tangentA = ( corner - LastPoint.point ).normalized;
			Vector3 tangentB = ( next - corner ).normalized;
			Vector3 cross = Vector3.Cross( tangentA, tangentB );

			if( cross.TaxicabMagnitude() <= 0.001f ) {
				AddPoint( corner, color ); // straight line
				return; // pretty much just a straight line. only add the corner point
			}

			Vector3 axis = cross.normalized;
			Vector3 normA = Vector3.Cross( axis, tangentA ); // normalized
			Vector3 normB = Vector3.Cross( axis, tangentB );
			Vector3 cornerDir = ( normA + normB ).normalized;
			float cornerBDot = Vector3.Dot( cornerDir, normB );
			Vector3 center = corner + cornerDir * ( ( radius / cornerBDot ) );
			// calc count here if density based
			if( useDensity ) {
				float angTurn = Vector3.Angle( normA, normB ) / 360f;
				targetPointCount = Mathf.RoundToInt( angTurn * pointsPerTurn );
			}

			AddPoints( ShapesMath.GetArcPoints( -normA, -normB, center, radius, targetPointCount ), color );
		}

		#endregion

		public bool EnsureMeshIsReadyToRender( bool closed, PolylineJoins renderJoins, out Mesh outMesh ) {
			if( meshDirty == false ) {
				// polyline itself didn't change, but the render state might force us to update
				if( renderJoins != lastUsedJoins || closed != lastUsedClosed )
					meshDirty = true;
			}

			return base.EnsureMeshIsReadyToRender( out outMesh, () => { TryUpdateMesh( closed, renderJoins ); } );
		}

		void TryUpdateMesh( bool closed, PolylineJoins joins ) {
			lastUsedClosed = closed;
			lastUsedJoins = joins;
			// todo: be smarter about this, maybe don't mesh.clear but check point count and whatnot
			ShapesMeshGen.GenPolylineMesh( base.mesh, path, closed, joins, true );
		}


	}


}