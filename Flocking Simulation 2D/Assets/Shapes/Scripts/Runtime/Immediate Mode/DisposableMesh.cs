using System;
using UnityEngine;

// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
namespace Shapes {

	public class DisposableMesh : IDisposable {

		// mesh states
		protected Mesh mesh;
		protected bool meshDirty = false;
		protected bool hasSetFirstPoint;
		bool hasMesh = false; // used to detect if mesh needs to update on the fly on draw

		protected void OnSetFirstDataPoint() {
			hasSetFirstPoint = true;
			mesh = new Mesh() { hideFlags = HideFlags.DontSave };
			hasMesh = true;
		}

		public void Dispose() {
			if( hasSetFirstPoint )
				mesh.DestroyBranched();
		}

		protected virtual bool ExternallyDirty() => false;
		protected virtual void UpdateMesh() => _ = 0;

		protected bool EnsureMeshIsReadyToRender( out Mesh outMesh, Action updateMesh ) {
			if( hasMesh == false ) {
				// no mesh exists because no points were added
				outMesh = null;
				return false;
			}

			updateMesh();
			outMesh = mesh;
			return hasMesh;
		}

	}

}