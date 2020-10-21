using System.Linq;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	interface IParamSelector {
		int Variants { get; }
		Param[] GetVariant( int i );
	}

	class CombinatorialParams : IParamSelector {
		Param[] @params;
		public int Variants => 1 << @params.Length;
		public CombinatorialParams( params Param[] @params ) => this.@params = @params;

		public Param[] GetVariant( int i ) {
			uint bits = (uint)i;
			int paramCount = bits.PopCount();
			if( paramCount == 0 ) return null;
			Param[] retParams = new Param[paramCount];
			int p = 0;
			for( int j = 0; j < @params.Length; j++ ) {
				if( ( bits & ( 1 << j ) ) > 0 )
					retParams[p++] = @params[j];
			}

			return retParams;
		}
	}

	class OrSelectorParams : IParamSelector {
		Param[][] paramList;
		public int Variants => paramList.Length;
		public OrSelectorParams( params Param[][] paramList ) => this.paramList = paramList;
		public Param[] GetVariant( int i ) => paramList[i];
	}


	class Param : IParamSelector {
		public string methodSigType;
		public string methodSigName;
		public string methodCallStr;
		public string methodSigDefault = null;
		public string[] targetArgNames;

		public string FullMethodSig {
			get {
				string s = $"{methodSigType} {methodSigName}";
				if( methodSigDefault != null )
					s += $" = {methodSigDefault}";
				return s;
			}
		}

		public Param( string methodSigParam, params string[] targetArgNames ) {
			this.methodSigDefault = null;
			if( methodSigParam.Contains( "=" ) ) // this means we have a default value
				( methodSigParam, this.methodSigDefault ) = methodSigParam.Split( '=' ).Select( x => x.Trim() ).ToArray(); // this is fine don't judge me okay
			// methodSigParam is in the form "Vector3 start" now
			( this.methodSigType, this.methodSigName ) = methodSigParam.Split( ' ' ).Select( x => x.Trim() ).ToArray();
			this.targetArgNames = targetArgNames.Length == 0 ? new[] { this.methodSigName } : targetArgNames;
			methodCallStr = this.methodSigName; // default
		}

		public int Variants => 1;
		public Param[] GetVariant( int i ) => new[] { this };

		public static implicit operator Param( string s ) => new Param( s );
	}

}