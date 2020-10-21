using System;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public enum PolylineJoins {
		Simple,
		Miter,
		Round,
		Bevel
	}

	public static class PolylineJoinsExtensions {

		public static bool HasJoinMesh( this PolylineJoins join ) {
			switch( join ) {
				case PolylineJoins.Simple: return false;
				case PolylineJoins.Miter:  return false;
				case PolylineJoins.Round:  return true;
				case PolylineJoins.Bevel:  return true;
				default:                   throw new ArgumentOutOfRangeException( nameof(join), join, null );
			}
		}

		public static bool HasSimpleJoin( this PolylineJoins join ) {
			switch( join ) {
				case PolylineJoins.Simple: return false;
				case PolylineJoins.Miter:  return false;
				case PolylineJoins.Round:  return false;
				case PolylineJoins.Bevel:  return true;
				default:                   throw new ArgumentOutOfRangeException( nameof(join), join, null );
			}
		}

	}

}