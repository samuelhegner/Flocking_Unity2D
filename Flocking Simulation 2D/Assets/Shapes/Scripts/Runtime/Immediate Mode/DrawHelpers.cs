using System;
using System.Collections.Generic;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public static partial class Draw {

		public static Color Rgb {
			get {
				Color c = Color;
				c.a = 1f;
				return c;
			}
			set => Color = new Color( value.r, value.g, value.b, Color.a );
		}
		public static float Opacity {
			get => Color.a;
			set {
				Color c = Color;
				c.a = value;
				Color = c;
			}
		}
		public static void SetColorOpacity( Color rgb, float opacity ) => Color = new Color( rgb.r, rgb.g, rgb.b, rgb.a * opacity );

		public struct TemporaryColor : IDisposable {
			static Stack<Color> colorStack = new Stack<Color>();

			public TemporaryColor( Color newColor ) {
				colorStack.Push( Draw.Color );
				Draw.Color = newColor;
			}

			public TemporaryColor( Color rgb, float opacity ) {
				colorStack.Push( Draw.Color );
				Draw.Color = new Color( rgb.r, rgb.g, rgb.b, opacity );
			}

			void IDisposable.Dispose() => Draw.Color = colorStack.Pop();
		}

		public struct TemporaryOpacity : IDisposable {
			static Stack<float> opacityStack = new Stack<float>();

			public TemporaryOpacity( float newOpacity ) {
				opacityStack.Push( Draw.Opacity );
				Draw.Opacity = newOpacity;
			}

			void IDisposable.Dispose() => Draw.Opacity = opacityStack.Pop();
		}

	}

}