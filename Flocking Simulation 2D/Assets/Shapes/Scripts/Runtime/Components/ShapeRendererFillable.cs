using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public abstract class ShapeRendererFillable : ShapeRenderer {
		
		// global color fill gradient shenanigans
		#if UNITY_EDITOR
		public ShapeFill Fill => fill;
		#endif
		[SerializeField] protected ShapeFill fill = new ShapeFill();
		[SerializeField] protected bool useFill = false;
		protected int FillTypeShaderInt => useFill ? fill.GetShaderFillModeInt() : ShapeFill.FILL_NONE;
		public bool UseFill {
			get => useFill;
			set {
				useFill = value;
				SetIntNow( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
			}
		}
		public FillType FillType {
			get => fill.type;
			set {
				fill.type = value;
				SetIntNow( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
			}
		}
		public FillSpace FillSpace {
			get => fill.space;
			set => SetIntNow( ShapesMaterialUtils.propFillSpace, (int)( fill.space = value ) );
		}
		public Vector3 FillRadialOrigin {
			get => fill.radialOrigin;
			set {
				fill.radialOrigin = value;
				SetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
			}
		}
		public float FillRadialRadius {
			get => fill.radialRadius;
			set {
				fill.radialRadius = value;
				SetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
			}
		}
		public Vector3 FillLinearStart {
			get => fill.linearStart;
			set {
				fill.linearStart = value;
				SetVector4Now( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
			}
		}
		public Vector3 FillLinearEnd {
			get => fill.linearEnd;
			set => SetVector3Now( ShapesMaterialUtils.propFillEnd, fill.linearEnd = value );
		}
		public Color FillColorStart {
			get => fill.colorStart;
			set => SetColorNow( ShapesMaterialUtils.propColor, fill.colorStart = value );
		}
		public Color FillColorEnd {
			get => fill.colorEnd;
			set => SetColorNow( ShapesMaterialUtils.propColorEnd, fill.colorEnd = value );
		}

		protected void SetFillProperties() {
			if( useFill ) {
				SetInt( ShapesMaterialUtils.propFillSpace, (int)fill.space );
				SetVector4( ShapesMaterialUtils.propFillStart, fill.GetShaderStartVector() );
				SetVector3( ShapesMaterialUtils.propFillEnd, fill.linearEnd );
				SetColor( ShapesMaterialUtils.propColor, fill.colorStart );
				SetColor( ShapesMaterialUtils.propColorEnd, fill.colorEnd );
			}

			SetInt( ShapesMaterialUtils.propFillType, FillTypeShaderInt );
		}
		
		
		
	}

}