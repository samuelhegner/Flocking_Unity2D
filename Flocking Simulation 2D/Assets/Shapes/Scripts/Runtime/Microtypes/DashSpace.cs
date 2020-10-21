// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/

namespace Shapes {

	public enum DashSpace {
		FixedCount = -2,
		Relative = -1,
		Meters = 0 // this enum matches the thickness space enum, just, in case I add support for px/noot sizing here in the future
	}

	public static class DashExtensions {
		public static int GetIndex( this DashSpace noot ) => (int)noot + 2;
	}

}