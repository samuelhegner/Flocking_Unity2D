// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	public enum ShapesBlendMode {
		//					ZWrite	RenderType	Blending		AlphaToCoverage
		Opaque, //	        on		opaque		none			on
		Transparent, //	    off		transparent	alpha blend		off
		Additive, //        off		transparent additive		off
		Multiplicative, //  off		transparent multiply		off
	}

}