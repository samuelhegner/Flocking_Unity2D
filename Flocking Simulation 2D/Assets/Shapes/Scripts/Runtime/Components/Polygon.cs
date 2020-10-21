using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[ExecuteInEditMode]
	[AddComponentMenu( "Shapes/Polygon" )]
	public class Polygon : ShapeRendererFillable {

		#if UNITY_EDITOR
		[Obsolete( "Please use " + nameof(points) + " instead - this one is deprecated", error: true )]
		public List<Vector2> PolyPoints => points;
		#endif

		/// <summary>
		/// <remarks>IMPORTANT: if you modify this list, you need to set meshOutOfDate to true, otherwise your changes won't apply</remarks> 
		/// </summary>
		[FormerlySerializedAs( "polyPoints" )] [SerializeField] public List<Vector2> points = new List<Vector2>() {
			new Vector2( 1f, 0f ),
			new Vector2( 0.5f, 0.86602545f ),
			new Vector2( -0.5f, 0.8660254f ),
			new Vector2( -1f, 0f ),
			new Vector2( -0.5f, -0.86602545f ),
			new Vector2( 0.5f, -0.86602545f )
		};

		public bool meshOutOfDate = true; // todo: move this to base class?

		// also called alignment
		[SerializeField] PolygonTriangulation triangulation = PolygonTriangulation.EarClipping;
		public PolygonTriangulation Triangulation {
			get => triangulation;
			set {
				triangulation = value;
				meshOutOfDate = true;
			}
		}

		public int Count => points.Count;
		public Vector2 this[ int i ] {
			get => points[i];
			set {
				points[i] = value;
				meshOutOfDate = true;
			}
		}

		public void SetPointPosition( int index, Vector2 position ) {
			if( index < 0 || index >= Count ) throw new IndexOutOfRangeException();
			points[index] = position;
			meshOutOfDate = true;
		}

		public void SetPoints( IEnumerable<Vector2> points ) {
			this.points.Clear();
			AddPoints( points );
		}

		public void AddPoints( IEnumerable<Vector2> points ) {
			this.points.AddRange( points );
			meshOutOfDate = true;
		}

		public void AddPoint( Vector2 point ) {
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

		protected override void SetAllMaterialProperties() => SetFillProperties();

		public override bool HasScaleModes => false;
		protected override Material[] GetMaterials() => new[] { ShapesMaterialUtils.matPolygon[BlendMode] };
		protected override MeshUpdateMode MeshUpdateMode => MeshUpdateMode.SelfGenerated;

		protected override void GenerateMesh() => ShapesMeshGen.GenPolygonMesh( Mesh, points, triangulation );

		protected override Bounds GetBounds() {
			if( points.Count < 2 )
				return default;
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;
			foreach( Vector3 pt in points ) {
				min = Vector3.Min( min, pt );
				max = Vector3.Max( max, pt );
			}

			return new Bounds( ( max + min ) * 0.5f, max - min );
		}

	}

}