using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	[DisallowMultipleComponent]
	public abstract class ShapeRenderer : MonoBehaviour {

		bool initializedComponents = false;
		MeshRenderer rnd;
		MeshFilter mf;
		int meshOwnerID;
		MaterialPropertyBlock mpb;
		MaterialPropertyBlock Mpb => mpb ?? ( mpb = new MaterialPropertyBlock() ); // hecking, gosh, I want the C#8 ??= operator
		Material[] instancedMaterials = null; // used when pass tags are anything but the default (eg ZTest != Less Equal, or scale offset is set, or a weird blend mode)
		public bool IsUsingUniqueMaterials => IsInstanced == false;

		public Mesh Mesh {
			get => mf.sharedMesh;
			private set => mf.sharedMesh = value;
		}
		public int SortingLayerID {
			get => MakeSureComponentExists( ref rnd, out _ ).sortingLayerID;
			set => MakeSureComponentExists( ref rnd, out _ ).sortingLayerID = value;
		}
		public int SortingOrder {
			get => MakeSureComponentExists( ref rnd, out _ ).sortingOrder;
			set => MakeSureComponentExists( ref rnd, out _ ).sortingOrder = value;
		}
		public string SortingLayerName => SortingLayer.IDToName( SortingLayerID );


		// Properties
		[SerializeField] ShapesBlendMode blendMode = ShapesBlendMode.Transparent;
		public ShapesBlendMode BlendMode {
			get => blendMode;
			set {
				blendMode = value;
				UpdateMaterial();
			}
		}
		[SerializeField] ScaleMode scaleMode = ScaleMode.Uniform;
		public ScaleMode ScaleMode {
			get => scaleMode;
			set => SetIntNow( ShapesMaterialUtils.propScaleMode, (int)( scaleMode = value ) );
		}
		[SerializeField] [ColorUsage( true, ShapesConfig.USE_HDR_COLOR_PICKERS )] protected Color color = Color.white;
		public virtual Color Color {
			get => color;
			set => SetColorNow( ShapesMaterialUtils.propColor, color = value );
		}

		#region instancing breaking pass tags

		// if and only if all are set to their default values
		bool IsInstanced => zTest == CompareFunction.LessEqual && zOffsetFactor == 0f && zOffsetUnits == 0;
		[SerializeField] CompareFunction zTest = CompareFunction.LessEqual;
		public CompareFunction ZTest {
			get => zTest;
			set => SetIntNow( ShapesMaterialUtils.propZTest, (int)( zTest = value ) );
		}
		[SerializeField] float zOffsetFactor = 0f;
		public float ZOffsetFactor {
			get => zOffsetFactor;
			set => SetFloatNow( ShapesMaterialUtils.propZOffsetFactor, zOffsetFactor = value );
		}
		[SerializeField] int zOffsetUnits = 0;
		public int ZOffsetUnits {
			get => zOffsetUnits;
			set => SetIntNow( ShapesMaterialUtils.propZOffsetUnits, zOffsetUnits = value );
		}

		#endregion

		#if UNITY_EDITOR
		public virtual void OnValidate() {
			// OnValidate can get called before awake in editor, so make sure the required things are initialized
			if( rnd == null ) rnd = GetComponent<MeshRenderer>(); // Needed for ApplyProperties
			if( mf == null ) mf = GetComponent<MeshFilter>(); // Needed for UpdateMesh
			ShapeClampRanges();
			UpdateAllMaterialProperties();
			ApplyProperties();

			// UpdateMesh( force:true ); gosh I wish I could do this it would solve so many problems but Unity has some WEIRD quirks here
		}

		public void HideMeshFilterRenderer() {
			const HideFlags flags = HideFlags.HideInInspector; // Hide mesh renderer and filter
			rnd.hideFlags = flags;
			mf.hideFlags = flags;
		}
		#endif

		T MakeSureComponentExists<T>( ref T field, out bool created ) where T : Component {
			if( field == null ) {
				field = GetComponent<T>();
				if( field == null ) {
					field = gameObject.AddComponent<T>();
					created = true;
				}

				field.hideFlags = HideFlags.HideInInspector;
			}

			created = false;
			return field;
		}

		void VerifyComponents() {
			if( initializedComponents == false ) {
				initializedComponents = true;
				MakeSureComponentExists( ref mf, out _ );
				MakeSureComponentExists( ref rnd, out bool createdRnd );
				if( createdRnd ) {
					rnd.receiveShadows = false;
					rnd.shadowCastingMode = ShadowCastingMode.Off;
					rnd.lightProbeUsage = LightProbeUsage.Off;
					rnd.reflectionProbeUsage = ReflectionProbeUsage.Off;
				}
			}
		}

		public virtual void Awake() {
			VerifyComponents();
			UpdateMaterial();
			UpdateMesh();
			UpdateAllMaterialProperties();
		}

		bool HasGeneratedOrCopyOfMesh => MeshUpdateMode == MeshUpdateMode.SelfGenerated || MeshUpdateMode == MeshUpdateMode.UseAssetCopy;

		public virtual void OnEnable() {
			UpdateMesh();
			rnd.enabled = true;
			#if UNITY_EDITOR
			if( HasGeneratedOrCopyOfMesh )
				UnityEditor.Undo.undoRedoPerformed += UpdateMeshOnUndoRedo;
			#endif
			if( UseCamOnPreCull )
				SubscribeCamPreCull();
		}

		void OnDisable() {
			if( rnd != null )
				rnd.enabled = false;
			#if UNITY_EDITOR
			if( HasGeneratedOrCopyOfMesh )
				UnityEditor.Undo.undoRedoPerformed -= UpdateMeshOnUndoRedo;
			#endif
			if( UseCamOnPreCull )
				UnsubscribeCamPreCull();
		}

		// These are used for polyline and polygon to detect mesh changes and lazily update before rendering
		void OnPreCamCullWithCam( Camera cam ) => CamOnPreCull();

		#if UNITY_2019_1_OR_NEWER
		void OnPreCamCullWithCam( ScriptableRenderContext ctx, Camera cam ) => CamOnPreCull();
		#endif

		void SubscribeCamPreCull() {
			if( UnityInfo.UsingSRP ) {
				#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering += OnPreCamCullWithCam;
				#else
				UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += OnPreCamCullWithCam;
				#endif
			} else
				Camera.onPreCull += OnPreCamCullWithCam;
		}

		void UnsubscribeCamPreCull() {
			if( UnityInfo.UsingSRP ) {
				#if UNITY_2019_1_OR_NEWER
				RenderPipelineManager.beginCameraRendering -= OnPreCamCullWithCam;
				#else
				UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering -= OnPreCamCullWithCam;
				#endif
			} else
				Camera.onPreCull -= OnPreCamCullWithCam;
		}


		#if UNITY_EDITOR
		void UpdateMeshOnUndoRedo() => UpdateMesh( true );
		#endif

		void Reset() {
			UpdateAllMaterialProperties();
			UpdateMesh( true );
		}

		void OnDestroy() {
			if( HasGeneratedOrCopyOfMesh && Mesh != null )
				DestroyImmediate( Mesh );
			this.TryDestroyInOnDestroy( rnd );
			this.TryDestroyInOnDestroy( mf );
			TryDestroyInstancedMaterials( inOnDestroy: true );
		}


		protected virtual void DrawGizmos( bool selected ) => _ = 0;
		private void OnDrawGizmos() => DrawGizmos( false );
		private void OnDrawGizmosSelected() => DrawGizmos( true );
		protected abstract void SetAllMaterialProperties();
		protected virtual void ShapeClampRanges() => _ = 0;
		protected abstract Material[] GetMaterials();
		protected abstract Bounds GetBounds();
		protected virtual void GenerateMesh() => _ = 0;
		protected virtual Mesh GetInitialMeshAsset() => ShapesMeshUtils.QuadMesh;
		protected virtual MeshUpdateMode MeshUpdateMode => MeshUpdateMode.UseAsset;
		public virtual bool HasScaleModes => true;
		protected virtual bool UseCamOnPreCull => false;
		protected virtual void CamOnPreCull() => _ = 0;

		void UpdateMeshBounds() => Mesh.bounds = GetBounds();


		void TryDestroyInstancedMaterials( bool inOnDestroy = false ) {
			if( instancedMaterials != null ) {
				for( int i = 0; i < instancedMaterials.Length; i++ ) {
					if( instancedMaterials[i] != null ) {
						if( inOnDestroy )
							this.TryDestroyInOnDestroy( instancedMaterials[i] );
						else
							instancedMaterials[i].DestroyBranched();
					}
				}
			}
		}

		void MakeSureMaterialInstancesAreGood( Material[] sourceMats ) {
			Material InstantiateMaterial( int index ) => new Material( sourceMats[index] ) { name = sourceMats[index].name + " (instance)" };

			void PopulateAll() {
				instancedMaterials = new Material[sourceMats.Length];
				for( int i = 0; i < sourceMats.Length; i++ )
					instancedMaterials[i] = InstantiateMaterial( i );
			}

			if( instancedMaterials == null ) {
				// no instanced materials exist, create all
				PopulateAll();
			} else {
				if( instancedMaterials.Length != sourceMats.Length ) {
					// length mismatch, regenerate all
					TryDestroyInstancedMaterials();
					PopulateAll();
				} else {
					// same length! make sure they are all matching
					for( int i = 0; i < sourceMats.Length; i++ ) {
						if( instancedMaterials[i] == null ) {
							// if null, create instance
							instancedMaterials[i] = InstantiateMaterial( i );
						} else {
							// if not null, then make sure the shader is matching
							if( instancedMaterials[i].shader != sourceMats[i].shader ) {
								// mismatch, destroy instance and assign new one
								instancedMaterials[i].DestroyBranched();
								instancedMaterials[i] = InstantiateMaterial( i );
							} else {
								// they do use the same shader, but, make sure they also use the same keywords just to be safe
								instancedMaterials[i].shaderKeywords = sourceMats[i].shaderKeywords;
							}
						}
					}
				}
			}
		}

		protected void UpdateMaterial() {
			Material[] targetMats = GetMaterials();

			// this means we have unique material properties for this shape, so, instantiate them
			// but! only when they're in the scene
			if( IsUsingUniqueMaterials ) {
				MakeSureMaterialInstancesAreGood( targetMats );
				targetMats = instancedMaterials;
			}

			#if UNITY_EDITOR
			bool needsUpdate = false;
			if( rnd.sharedMaterials.Length != targetMats.Length ) {
				needsUpdate = true;
			} else {
				for( int i = 0; i < targetMats.Length; i++ ) {
					if( rnd.sharedMaterials[i] != targetMats[i] ) {
						string shMat = rnd.sharedMaterials[i] == null ? "null" : rnd.sharedMaterials[i].GetType().Name;
						needsUpdate = true;
						break;
					}
				}
			}

			if( needsUpdate ) {
				UnityEditor.SerializedObject soRnd = new UnityEditor.SerializedObject( rnd );
				UnityEditor.Undo.RecordObject( rnd, "" );
				rnd.sharedMaterials = targetMats;
			}
			#else
			rnd.sharedMaterials = targetMats;
			#endif
		}

		public void UpdateMesh( bool force = false ) {
			MeshUpdateMode mode = MeshUpdateMode;

			// if we're using a mesh asset, we only assign if it's null or mismatching
			if( mode == MeshUpdateMode.UseAsset && ( Mesh == null || Mesh != GetInitialMeshAsset() ) ) {
				Mesh = GetInitialMeshAsset();
				return;
			}

			// the next two modes are copy-sensitive, meaning that if we duplicate this object,
			// we also have to duplicate the mesh and update which mesh the duplicate is pointing to
			int id = gameObject.GetInstanceID();

			bool createMesh = Mesh == null || meshOwnerID != id;

			// create new mesh
			if( createMesh ) {
				meshOwnerID = id;
				if( mode == MeshUpdateMode.UseAssetCopy ) {
					Mesh = Instantiate( GetInitialMeshAsset() );
					Mesh.hideFlags = HideFlags.HideAndDontSave;
					Mesh.MarkDynamic();
				} else if( mode == MeshUpdateMode.SelfGenerated ) {
					Mesh = new Mesh() { hideFlags = HideFlags.HideAndDontSave };
					Mesh.MarkDynamic();
					GenerateMesh();
				}
			} else if( force && mode == MeshUpdateMode.SelfGenerated ) {
				GenerateMesh(); // update existing mesh
			}
		}

		public Bounds GetWorldBounds() {
			Bounds localBounds = GetBounds();
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;

			Transform tf = transform;
			for( int x = -1; x <= 1; x += 2 )
				for( int y = -1; y <= 1; y += 2 )
					for( int z = -1; z <= 1; z += 2 ) {
						Vector3 wPt = tf.TransformPoint( localBounds.center + Vector3.Scale( localBounds.extents, new Vector3( x, y, z ) ) );
						min = Vector3.Min( min, wPt );
						max = Vector3.Max( max, wPt );
					}

			return new Bounds( ( max + min ) / 2f, max - min );
		}

		void OnDidApplyAnimationProperties() => UpdateAllMaterialProperties(); // so this is not great but it works don't judge

		public void UpdateAllMaterialProperties() {
			if( gameObject.scene.IsValid() == false )
				return; // not in a scene :c

			UpdateMaterial();

			if( IsUsingUniqueMaterials ) // I wish we could material property block these ;-;
				foreach( Material instancedMaterial in instancedMaterials ) {
					instancedMaterial.SetInt( ShapesMaterialUtils.propZTest, (int)zTest );
					instancedMaterial.SetFloat( ShapesMaterialUtils.propZOffsetFactor, zOffsetFactor );
					instancedMaterial.SetInt( ShapesMaterialUtils.propZOffsetUnits, zOffsetUnits );
				}

			SetColor( ShapesMaterialUtils.propColor, color );
			if( HasScaleModes )
				SetInt( ShapesMaterialUtils.propScaleMode, (int)scaleMode );
			SetAllMaterialProperties();
			ApplyProperties();
		}

		protected void ApplyProperties() {
			VerifyComponents(); // make sure components exists. rnd can be uninitialized if you modify an object that has never had awake called
			rnd.SetPropertyBlock( Mpb );
			if( MeshUpdateMode == MeshUpdateMode.UseAssetCopy )
				UpdateMeshBounds();
		}


		protected void SetColor( int prop, Color value ) {
			if( ShapeGroup.shapeGroupsInScene > 0 ) { // if color tint groups exist, see if we have any
				ShapeGroup[] groups = GetComponentsInParent<ShapeGroup>();
				if( groups != null )
					foreach( ShapeGroup shapeGroup in groups.Where( g => g.IsEnabled ) )
						value *= shapeGroup.Color;
			}

			Mpb.SetColor( prop, value );
		}

		protected void SetFloat( int prop, float value ) => Mpb.SetFloat( prop, value );
		protected void SetInt( int prop, int value ) => Mpb.SetInt( prop, value );
		protected void SetVector3( int prop, Vector3 value ) => Mpb.SetVector( prop, value );
		protected void SetVector4( int prop, Vector4 value ) => Mpb.SetVector( prop, value );

		protected void SetColorNow( int prop, Color value ) {
			SetColor( prop, value );
			ApplyProperties();
		}

		protected void SetFloatNow( int prop, float value ) {
			Mpb.SetFloat( prop, value );
			ApplyProperties();
		}

		protected void SetIntNow( int prop, int value ) {
			Mpb.SetInt( prop, value );
			ApplyProperties();
		}

		protected void SetVector3Now( int prop, Vector3 value ) {
			Mpb.SetVector( prop, value );
			ApplyProperties();
		}

		protected void SetVector4Now( int prop, Vector4 value ) {
			Mpb.SetVector( prop, value );
			ApplyProperties();
		}


	}

}