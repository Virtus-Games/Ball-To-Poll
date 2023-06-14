namespace JujubeMapEditor.Test {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using JujubeMapEditor.Core;


	// Example script for custom block
	public class JBlockExample : JBlock {


		// Called everytime when block been placed on map by brush or reload.
		public override void OnBlockLoaded (JujubeRenderer renderer, JujubeBlock block) {
			if (renderer == null || block == null) { return; }
			Debug.Log($"[JBlockExample] Loaded by {renderer.name} At Position: {block.Position}");


			// ...Your Code Here...


		}


	}
}