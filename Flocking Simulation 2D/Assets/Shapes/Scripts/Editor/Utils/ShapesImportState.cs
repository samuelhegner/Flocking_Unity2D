#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class ShapesImportState : ScriptableObject {
		[Header( "Do not edit this~" )]
		public RenderPipeline currentShaderRP = RenderPipeline.Legacy;

		static ShapesImportState instance;
		public static ShapesImportState Instance => instance != null ? instance : instance = Resources.Load<ShapesImportState>( "Shapes Import State" );

		#if UNITY_EDITOR
		[DidReloadScripts( 1 )]
		public static void CheckRenderPipelineSoon() => EditorApplication.delayCall += CheckRenderPipeline;

		static void CheckRenderPipeline() {
			RenderPipeline rpInUnity = UnityInfo.GetCurrentRenderPipelineInUse();
			ShapesImportState inst = Instance;
			if( inst == null ) {
				Debug.LogWarning( "Failed to detect render pipeline - Shapes will retry on the next script recompile" );
				return; // I guess some weird import order shenan happened? :c
			}

			RenderPipeline rpShapesShaders = inst.currentShaderRP;
			if( rpInUnity != rpShapesShaders ) {
				string rpStr = rpInUnity.ToString();
				if( rpInUnity == RenderPipeline.Legacy )
					rpStr = "the built-in render pipeline";
				string desc = $"Looks like you're using {rpStr}!\nShapes will now regenerate all shaders, it might take a lil while~";
				EditorUtility.DisplayDialog( "Shapes", desc, "ok" );
				CodegenShaders.GenerateShadersAndMaterials();
			}
		}
		#endif

	}

}