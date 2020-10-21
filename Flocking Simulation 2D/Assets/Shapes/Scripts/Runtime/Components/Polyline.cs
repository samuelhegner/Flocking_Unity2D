using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Polyline" )]
	public class Polyline : ShapeRenderer {

		#if UNITY_EDITOR
		[Obsolete( "Please use " + nameof(points) + " instead - this one is deprecated", error: true )]
		public List<PolylinePoint> PolyPoints => points;
		#endif

		/// <summary>
		/// <remarks>IMPORTANT: if you modify this list, you need to set meshOutOfDate to true, otherwise your changes won't apply</remarks> 
		/// </summary>
		[FormerlySerializedAs( "polyPoints" )] [SerializeField] public List<PolylinePoint> points = new List<PolylinePoint>() {
			new PolylinePoint( new Vector3( 0, 1, 0 ), Color.white ),
			new PolylinePoint( new Vector3( 0.86602540378f, -.5f, 0 ), Color.white ),
			new PolylinePoint( new Vector3( -0.86602540378f, -.5f, 0 ), Color.white )
		};

		public bool meshOutOfDate = true; // todo: move this to base class?

		// also called alignment
		[SerializeField] PolylineGeometry geometry = PolylineGeometry.Flat2D;
		public PolylineGeometry Geometry {
			get => geometry;
			set {
				geometry = value;
				SetIntNow( ShapesMaterialUtils.propAlignment, (int)geometry );
				UpdateMaterial();
				ApplyProperties();
			}
		}

		[SerializeField] PolylineJoins joins = PolylineJoins.Miter;
		public PolylineJoins Joins {
			get => joins;
			set {
				joins = value;
				meshOutOfDate = true;
			}
		}

		[SerializeField] bool closed = true;
		public bool Closed {
			get => closed;
			set {
				closed = value;
				meshOutOfDate = true;
			}
		}

		[SerializeField] float thickness = 0.125f;
		public float Thickness {
			get => thickness;
			set => SetFloatNow( ShapesMaterialUtils.propThickness, thickness = value );
		}

		// todo: make this work
		[SerializeField] ThicknessSpace thicknessSpace = Shapes.ThicknessSpace.Meters;
		public ThicknessSpace ThicknessSpace {
			get => thicknessSpace;
			set => SetIntNow( ShapesMaterialUtils.propThicknessSpace, (int)( thicknessSpace = value ) );
		}

		public int Count => points.Count;
		public PolylinePoint this[ int i ] {
			get => points[i];
			set {
				points[i] = value;
				meshOutOfDate = true;
			}
		}

		public void SetPointPosition( int index, Vector3 position ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			PolylinePoint pp = points[index];
			pp.point = position;
			points[index] = pp;
			meshOutOfDate = true;
		}

		public void SetPointColor( int index, Color color ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			PolylinePoint pp = points[index];
			pp.color = color;
			points[index] = pp;
			meshOutOfDate = true;
		}

		public void SetPointThickness( int index, float thickness ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			PolylinePoint pp = points[index];
			pp.thickness = thickness;
			points[index] = pp;
			meshOutOfDate = true;
		}

		public void SetPoints( IReadOnlyCollection<Vector3> points, IReadOnlyCollection<Color> colors = null ) {
			this.points.Clear();
			if( colors == null ) {
				AddPoints( points.Select( p => new PolylinePoint( p, Color.white ) ) );
			} else {
				if( points.Count != colors.Count )
					throw new ArgumentException( "point.Count != color.Count" );
				AddPoints( points.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
			}
		}

		public void SetPoints( IReadOnlyCollection<Vector2> points, IReadOnlyCollection<Color> colors = null ) {
			this.points.Clear();
			if( colors == null ) {
				AddPoints( points.Select( p => new PolylinePoint( p, Color.white ) ) );
			} else {
				if( points.Count != colors.Count )
					throw new ArgumentException( "point.Count != color.Count" );
				AddPoints( points.Zip( colors, ( p, c ) => new PolylinePoint( p, c ) ) );
			}
		}

		public void SetPoints( IEnumerable<PolylinePoint> points ) {
			this.points.Clear();
			AddPoints( points );
		}

		public void AddPoints( IEnumerable<PolylinePoint> points ) {
			this.points.AddRange( points );
			meshOutOfDate = true;
		}

		public void AddPoint( Vector3 position ) => AddPoint( new PolylinePoint( position ) );
		public void AddPoint( Vector3 position, Color color ) => AddPoint( new PolylinePoint( position, color ) );
		public void AddPoint( Vector3 position, Color color, float thickness ) => AddPoint( new PolylinePoint( position, color, thickness ) );
		public void AddPoint( Vector3 position, float thickness ) => AddPoint( new PolylinePoint( position, Color.white, thickness ) );

		public void AddPoint( PolylinePoint point ) {
			points.Add( point );
			meshOutOfDate = true;
		}

		protected override bool UseCamOnPreCull => true;

		protected override void CamOnPreCull() {
			if( meshOutOfDate ) {
				meshOutOfDate = false;
				UpdateMesh( force: true );
			}
		}


		protected override MeshUpdateMode MeshUpdateMode => MeshUpdateMode.SelfGenerated;
		protected override void GenerateMesh() => ShapesMeshGen.GenPolylineMesh( Mesh, points, closed, joins, true );

		protected override void SetAllMaterialProperties() {
			SetFloat( ShapesMaterialUtils.propThickness, thickness );
			SetInt( ShapesMaterialUtils.propThicknessSpace, (int)thicknessSpace );
			SetInt( ShapesMaterialUtils.propAlignment, (int)geometry );
		}

		protected override void ShapeClampRanges() => thickness = Mathf.Max( 0f, thickness );

		protected override Material[] GetMaterials() {
			if( joins.HasJoinMesh() )
				return new[] { ShapesMaterialUtils.GetPolylineMat( joins )[BlendMode], ShapesMaterialUtils.GetPolylineJoinsMat( joins )[BlendMode] };
			return new[] { ShapesMaterialUtils.GetPolylineMat( joins )[BlendMode] };
		}

		// todo: this doesn't take point thickness or thickness space into account
		protected override Bounds GetBounds() {
			if( points.Count < 2 )
				return default;
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;
			foreach( Vector3 pt in points.Select( p => p.point ) ) {
				min = Vector3.Min( min, pt );
				max = Vector3.Max( max, pt );
			}

			return new Bounds( ( max + min ) * 0.5f, ( max - min ) + Vector3.one * ( thickness * 0.5f ) );
		}

	}

}