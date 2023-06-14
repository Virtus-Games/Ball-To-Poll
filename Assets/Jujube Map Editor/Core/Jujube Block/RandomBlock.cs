namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using JujubeMapEditor.Core;




	[AddComponentMenu("Jujube/Random Block")]
	public class RandomBlock : JBlock {



		// VAR
		[SerializeField] private Transform[] m_Prefabs = null;
		[SerializeField] private bool RandomRotation = true;


		// MSG
		public override string GetPaletteLabel () => m_Prefabs != null ? m_Prefabs.Length.ToString() : "0";


		public override void OnBlockLoaded (JujubeRenderer _, JujubeBlock block) {

			if (m_Prefabs == null || m_Prefabs.Length == 0 || block == null) { return; }

			// Spawn
			string data = block.Data;
			if (string.IsNullOrEmpty(data) || !int.TryParse(data, out int index)) {
				// Random
				index = Random.Range(0, m_Prefabs.Length);
				block.Data = index.ToString();
				if (RandomRotation) {
					block.RotZ = Random.Range(0, 4);
					transform.localRotation = Quaternion.Euler(0f, block.RotZ * 90f, 0f);
				}
			}

			// Load
			if (transform.childCount > 0 && transform.GetChild(0).name != index.ToString()) {
				DestroyAllChirldrenImmediate(transform);
				index = Mathf.Clamp(index, 0, m_Prefabs.Length - 1);
				var prefab = m_Prefabs[index];
				if (prefab != null) {
					var tf = Instantiate(prefab.gameObject, transform).transform;
					tf.name = index.ToString();
					tf.localPosition = Vector3.zero;
					tf.localRotation = Quaternion.identity;
					tf.localScale = Vector3.one;
				}
			}

			// Destroy
			DestroyImmediate(this, false);

		}


		// LGC
		private void DestroyAllChirldrenImmediate (Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}


	}
}