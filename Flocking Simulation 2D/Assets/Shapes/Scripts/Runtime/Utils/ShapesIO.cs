using System.Linq;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#endif

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class ShapesIO {
		#if UNITY_EDITOR

		static string rootFolder = null;
		public static string RootFolder {
			get {
				if( rootFolder == null ) {
					string shapeAssetsPath = AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( "t:ShapesAssets" )[0] );
					int fileNameLen = "/Resources/Shapes Assets.asset".Length;
					rootFolder = shapeAssetsPath.Substring( 0, shapeAssetsPath.Length - fileNameLen );
				}

				return rootFolder;
			}
		}

		static string shaderFolder = null;
		public static string ShaderFolder {
			get {
				if( shaderFolder == null )
					shaderFolder = RootFolder + "/Shaders";
				return shaderFolder;
			}
		}
		public static string CoreShaderFolder => ShaderFolder + "/Core";
		public static string GeneratedMaterialsFolder => ShaderFolder + "/Generated Materials";
		public static string GeneratedShadersFolder => ShaderFolder + "/Generated Shaders/Resources";

		static string ConfigCsharpPath => RootFolder + "/Scripts/Runtime/ShapesConfig.cs";
		static string ConfigShadersPath => ShaderFolder + "/Shapes Config.cginc";

		public static void OpenConfigCsharp() => OpenAssetAtPath( ConfigCsharpPath );
		public static void OpenConfigShaders() => OpenAssetAtPath( ConfigShadersPath );

		static void OpenAssetAtPath( string path ) => AssetDatabase.OpenAsset( AssetDatabase.LoadAssetAtPath<Object>( path ) );

		static T LoadAssetWithGUID<T>( string guid ) where T : Object {
			string path = AssetDatabase.GUIDToAssetPath( guid );
			return AssetDatabase.LoadAssetAtPath<T>( path );
		}

		static string[] FindAllAssetGUIDs<T>() where T : Object {
			return AssetDatabase.FindAssets( $"t:{typeof(T).Name}" );
		}

		static string[] FindAllAssetGUIDs<T>( string path ) where T : Object {
			return AssetDatabase.FindAssets( $"t:{typeof(T).Name}", new[] { path } );
		}

		public static T TryLoadSingletonAsset<T>() where T : Object {
			string[] guids = FindAllAssetGUIDs<T>();
			return guids.Length > 0 ? LoadAssetWithGUID<T>( guids[0] ) : null;
		}

		public static IEnumerable<T> LoadAllAssets<T>( string path ) where T : Object {
			return FindAllAssetGUIDs<T>( path ).Select( LoadAssetWithGUID<T> );
		}

		static IEnumerable<T> LoadAllAssets<T>() where T : Object {
			return FindAllAssetGUIDs<T>().Select( LoadAssetWithGUID<T> );
		}

		#endif
	}

}