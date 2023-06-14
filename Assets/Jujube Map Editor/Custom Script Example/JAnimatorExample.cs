namespace JujubeMapEditor.Test {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using JujubeMapEditor.Core;


	// Example script for custom animator
	public class JAnimatorExample : JujubeAnimator {


		// Logic for block spreading
		// Param: block position in range 0(left,down,back) to 1(right,up,forward)
		// Return: block appear time in range 0(begin) to 1(end)
		protected override float GetSpreadAmount (Vector3 pos) {


			// ...Your Code Here...


			return pos.z;
		}


		// Logic for each block transform animation
		// Param: blockTF the block transform
		// Param: amount current spread amount for this block in range -1(hide) to 1(show)
		// Return: block state 1(active), -1(inactive)
		protected override int SetSpreadAmount (Transform blockTF, float amount) {


			// ...Your Code Here...


			blockTF.gameObject.SetActive(amount > 0f);
			return amount > 0f ? 1 : -1;
		}


	}
}