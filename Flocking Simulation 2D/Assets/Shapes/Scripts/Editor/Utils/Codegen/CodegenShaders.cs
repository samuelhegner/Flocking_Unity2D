using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public struct MultiCompile {
		string[] keywords;
		public int Count => keywords.Length + 1; // include 0th;
		public string this[ int i ] => i == 0 ? "" : keywords[i - 1];
		public MultiCompile( params string[] keywords ) => this.keywords = keywords;
		public override string ToString() => $"#pragma multi_compile __ {string.Join( " ", keywords )}";

		public IEnumerable<string> Enumerate() {
			for( int i = 0; i < Count; i++ )
				yield return this[i];
		}
	}

	public static class CodegenShaders {


		public static void GenerateShadersAndMaterials() {
			RenderPipeline currentRP = UnityInfo.GetCurrentRenderPipelineInUse();

			int blendModeCount = System.Enum.GetNames( typeof(ShapesBlendMode) ).Length;
			const string CORE_SUFFIX = " Core";
			IEnumerable<TextAsset> shaderCores = ShapesIO.LoadAllAssets<TextAsset>( ShapesIO.CoreShaderFolder );
			IEnumerable<string> shaderNames = shaderCores.Select( x => x.name.Substring( 0, x.name.Length - CORE_SUFFIX.Length ) );

			// generate all shaders
			foreach( string name in shaderNames ) {
				for( int i = 0; i < blendModeCount; i++ ) {
					ShapesBlendMode blendMode = (ShapesBlendMode)i;
					string path = $"{ShapesIO.GeneratedShadersFolder}/{name} {blendMode}.shader";
					string shaderContents = new ShaderBuilder( name, blendMode, currentRP ).shader;
					File.WriteAllText( path, shaderContents );
				}
			}

			// update the current shader state
			ShapesImportState.Instance.currentShaderRP = currentRP;
			EditorUtility.SetDirty( ShapesImportState.Instance );

			// reimport all assets to load newly generated shaders
			AssetDatabase.Refresh( ImportAssetOptions.Default );

			// generate all materials
			foreach( string name in shaderNames ) {
				for( int i = 0; i < blendModeCount; i++ ) {
					ShapesBlendMode blendMode = (ShapesBlendMode)i;
					string nameWithBlendMode = ShapesMaterials.GetMaterialName( name, blendMode.ToString() );
					Shader shader = Shader.Find( $"Shapes/{nameWithBlendMode}" );
					if( shader == null ) {
						Debug.LogError( "missing shader " + $"Shapes/{nameWithBlendMode}" );
						continue;
					}


					if( ShaderBuilder.shaderKeywords.ContainsKey( name ) ) {
						// create all permutations
						MultiCompile[] multis = ShaderBuilder.shaderKeywords[name];
						List<string> keywordPermutations = new List<string>();
						foreach( IEnumerable<string> perm in GetPermutations( multis.Select( m => m.Enumerate() ) ) ) {
							IEnumerable<string> validKeywords = perm.Where( p => string.IsNullOrEmpty( p ) == false );
							string kws = $" [{string.Join( "][", validKeywords )}]";
							if( kws.Contains( "[]" ) ) // this means it has no permutations
								kws = "";
							TryCreateMaterial( nameWithBlendMode + kws, validKeywords );
						}
					} else {
						TryCreateMaterial( nameWithBlendMode );
					}

					Material TryCreateMaterial( string fullMaterialName, IEnumerable<string> keywords = null ) {
						string savePath = $"{ShapesIO.GeneratedMaterialsFolder}/{fullMaterialName}.mat";
						Material mat = AssetDatabase.LoadAssetAtPath<Material>( savePath );

						void TrySetKeywordsAndDefaultProperties() {
							if( keywords != null ) {
								foreach( string keyword in keywords )
									mat.EnableKeyword( keyword );
							}
							ShapesMaterials.ApplyDefaultGlobalProperties( mat );
						}

						if( mat != null ) {
							EditorUtility.SetDirty( mat );
							mat.hideFlags = HideFlags.HideInInspector;
							TrySetKeywordsAndDefaultProperties();
						} else {
							Debug.Log( "creating material " + savePath );
							mat = new Material( shader ) { enableInstancing = true, hideFlags = HideFlags.HideInInspector };
							TrySetKeywordsAndDefaultProperties();
							AssetDatabase.CreateAsset( mat, savePath );
						}

						return mat;
					}
				}
			}

			AssetDatabase.Refresh( ImportAssetOptions.Default );
		}

		// magic wand wau ✨
		public static IEnumerable<IEnumerable<string>> GetPermutations( IEnumerable<IEnumerable<string>> inputData ) {
			IEnumerable<IEnumerable<string>> emptyProduct = new[] { Enumerable.Empty<string>() };
			return inputData.Aggregate( emptyProduct, ( accumulator, sequence ) => accumulator.SelectMany( accseq => sequence, ( accseq, item ) => accseq.Concat( new[] { item } ) ) );
		}

	}

}