using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesMeshUtils {

		static Vector3[] quadVerts = new[] { new Vector3( -1, -1 ), new Vector3( -1, 1 ), new Vector3( 1, 1 ), new Vector3( 1, -1 ) };
		static Vector3[] quadNormals = new[] { new Vector3( 0, 0, -1 ), new Vector3( 0, 0, -1 ), new Vector3( 0, 0, -1 ), new Vector3( 0, 0, -1 ) };
		static Vector2[] quadUvs = new[] { new Vector2( -1, -1 ), new Vector2( -1, 1 ), new Vector2( 1, 1 ), new Vector2( 1, -1 ) };
		static Color[] quadColors = new[] { new Color( 1, 0, 0, 0 ), new Color( 0, 1, 0, 0 ), new Color( 0, 0, 1, 0 ), new Color( 0, 0, 0, 1 ) };
		static int[] quadTris = new[] { 0, 1, 2, 0, 2, 3 };
		[DestroyOnAssemblyReload] static Mesh quadMesh;
		public static Mesh QuadMesh {
			get {
				if( quadMesh == null ) {
					quadMesh = new Mesh {
						hideFlags = HideFlags.HideAndDontSave,
						name = "Quad"
					};
					quadMesh.vertices = quadVerts;
					quadMesh.uv = quadUvs;
					quadMesh.triangles = quadTris;
					quadMesh.colors = quadColors;
					quadMesh.normals = quadNormals;
					quadMesh.bounds = ShapesConfig.BOUNDS_QUAD_PRIMITIVE;
				}

				return quadMesh;
			}
		}
		static Vector3[] triangleVerts = new[] { new Vector3( 1, 0, 0 ), new Vector3( 0, 1, 0 ), new Vector3( 0, 0, 1 ) };
		static int[] triangleTris = new[] { 0, 1, 2 };
		[DestroyOnAssemblyReload] static Mesh triangleMesh;
		public static Mesh TriangleMesh {
			get {
				if( triangleMesh == null ) {
					triangleMesh = new Mesh {
						hideFlags = HideFlags.HideAndDontSave,
						name = "Triangle"
					};
					triangleMesh.vertices = triangleVerts;
					triangleMesh.triangles = triangleTris;
					triangleMesh.bounds = ShapesConfig.BOUNDS_TRIANGLE_PRIMITIVE;
				}

				return triangleMesh;
			}
		}

		static Mesh sphereMesh;
		public static Mesh SphereMesh => sphereMesh == null ? ( sphereMesh = EnsureValidMeshBounds( ShapesAssets.Instance.meshSphere, ShapesConfig.BOUNDS_SPHERE_PRIMITIVE ) ) : sphereMesh;

		static Mesh cuboidMesh;
		public static Mesh CuboidMesh => cuboidMesh == null ? ( cuboidMesh = EnsureValidMeshBounds( ShapesAssets.Instance.meshCube, ShapesConfig.BOUNDS_CUBOID_PRIMITIVE ) ) : cuboidMesh;

		static Mesh torusMesh;
		public static Mesh TorusMesh => torusMesh == null ? ( torusMesh = EnsureValidMeshBounds( ShapesAssets.Instance.meshTorus, ShapesConfig.BOUNDS_TORUS_PRIMITIVE ) ) : torusMesh;

		static Mesh coneMesh;
		public static Mesh ConeMesh => coneMesh == null ? ( coneMesh = EnsureValidMeshBounds( ShapesAssets.Instance.meshCone, ShapesConfig.BOUNDS_CONE_PRIMITIVE ) ) : coneMesh;

		static Mesh coneMeshUncapped;
		public static Mesh ConeMeshUncapped => coneMeshUncapped == null ? ( coneMeshUncapped = EnsureValidMeshBounds( ShapesAssets.Instance.meshConeUncapped, ShapesConfig.BOUNDS_CONE_PRIMITIVE ) ) : coneMeshUncapped;

		static Mesh cylinderMesh;
		public static Mesh CylinderMesh => cylinderMesh == null ? ( cylinderMesh = EnsureValidMeshBounds( ShapesAssets.Instance.meshCylinder, ShapesConfig.BOUNDS_CYLINDER_PRIMITIVE ) ) : cylinderMesh;

		static Mesh capsuleMesh;
		public static Mesh CapsuleMesh => capsuleMesh == null ? ( capsuleMesh = EnsureValidMeshBounds( ShapesAssets.Instance.meshCapsule, ShapesConfig.BOUNDS_CAPSULE_PRIMITIVE ) ) : capsuleMesh;

		#if UNITY_EDITOR

		static ShapesMeshUtils() => AssemblyReloadEvents.beforeAssemblyReload += OnPreAssemblyReload;

		static void OnPreAssemblyReload() {
			AssemblyReloadEvents.beforeAssemblyReload -= OnPreAssemblyReload;
			BindingFlags bfs = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			bool IsDestroyTarget( FieldInfo f ) => f.GetCustomAttributes( typeof(DestroyOnAssemblyReload), false ).Length > 0 && f.GetValue( null ) != null;

			foreach( FieldInfo field in typeof(ShapesMeshUtils).GetFields( bfs ).Where( IsDestroyTarget ) ) {
				Object obj = (Object)field.GetValue( null );
				Object.DestroyImmediate( obj );
			}
		}

		#endif


		static Mesh EnsureValidMeshBounds( Mesh mesh, Bounds bounds ) {
			mesh.hideFlags = HideFlags.HideInInspector;
			mesh.bounds = bounds;
			return mesh;
		}

		public static Mesh GetLineMesh( LineGeometry geometry, LineEndCap endCaps ) {
			switch( geometry ) {
				case LineGeometry.Billboard:
				case LineGeometry.Flat2D:
					return QuadMesh;
				case LineGeometry.Volumetric3D:
					return endCaps == LineEndCap.Round ? CapsuleMesh : CylinderMesh;
			}

			return default;
		}

	}

}