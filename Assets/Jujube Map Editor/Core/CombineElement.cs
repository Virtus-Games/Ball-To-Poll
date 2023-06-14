namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	public enum CombineElementMode {
		Combine = 0,
		CombineWithoutOverlapNearbyFaces = 1,
		DoNotCombine = 2,
	}
	public class CombineElement : MonoBehaviour {
		public CombineElementMode CombineMode = CombineElementMode.Combine;
	}
}