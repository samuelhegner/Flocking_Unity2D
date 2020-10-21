using System;
using System.Collections.Generic;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static class RenderPipelineExtensions {

		static string PipelineSubshaderTagValue( this RenderPipeline rp ) {
			switch( rp ) {
				case RenderPipeline.Legacy: return "";
				case RenderPipeline.URP:    return "UniversalPipeline";
				case RenderPipeline.HDRP:   return "HDRenderPipeline";
				default:                    throw new ArgumentOutOfRangeException();
			}
		}

		public static IEnumerable<string> GetSubshaderTags( this RenderPipeline rp ) {
			if( rp == RenderPipeline.Legacy )
				yield break; // this is due to a bug where SRP sometimes pick the legacy pipeline. Putting it last and without a tag fixes this 
			yield return (ShaderTag)( "RenderPipeline", rp.PipelineSubshaderTagValue() );
		}

	}

}