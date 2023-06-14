namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class LayerFixingBlock : JBlock {



		// SUB
		public enum CompareMode {
			EqualToTarget = 0,
			StartWithTarget = 1,
			EndWithTarget = 2,
			ContainsTarget = 3,

		}


		// Ser
		[SerializeField] private string m_TargetLayer = "Layer Name";
		[SerializeField] private CompareMode m_Compare = CompareMode.EqualToTarget;
		[SerializeField] private bool m_LogMessage = true;


		// MSG
		public override string GetPaletteLabel () => "L";


		public override void OnBlockLoaded (JujubeRenderer renderer, JujubeBlock block) {

			int targetLayerIndex = -1;
			if (renderer != null && block != null && renderer.Map != null && renderer.Mode == JujubeRendererMode.Develop) {
				int layerCount = renderer.Map.LayerCount;
				for (int i = 0; i < layerCount; i++) {
					bool bingo = false;
					switch (m_Compare) {
						case CompareMode.EqualToTarget:
							if (renderer.Map[i].LayerName == m_TargetLayer) {
								bingo = true;
							}
							break;
						case CompareMode.StartWithTarget:
							if (renderer.Map[i].LayerName.StartsWith(m_TargetLayer)) {
								bingo = true;
							}
							break;
						case CompareMode.EndWithTarget:
							if (renderer.Map[i].LayerName.EndsWith(m_TargetLayer)) {
								bingo = true;
							}
							break;
						case CompareMode.ContainsTarget:
							if (renderer.Map[i].LayerName.IndexOf(m_TargetLayer) >= 0) {
								bingo = true;
							}
							break;
					}
					if (bingo) {
						targetLayerIndex = i;
						break;
					}
				}
				if (targetLayerIndex >= 0) {
					int currentLayerIndex = transform.parent.GetSiblingIndex();
					var targetLayerTF = renderer.GetLayerTF(targetLayerIndex);
					if (targetLayerIndex != currentLayerIndex && targetLayerTF != null && currentLayerIndex < layerCount) {
						var currentLayer = renderer.Map[currentLayerIndex];
						int currentBlockIndex = transform.GetSiblingIndex();
						if (currentBlockIndex < currentLayer.BlockCount) {
							currentLayer.Blocks.RemoveAt(currentBlockIndex);
							renderer.Map[targetLayerIndex].Blocks.Add(block);
							SaveMap(renderer.Map);
							transform.SetParent(targetLayerTF);
							transform.SetAsLastSibling();
							if (m_LogMessage) {
								Debug.Log($"[Jujube Map Editor] Fixing layer from {currentLayerIndex} to {targetLayerIndex}.");
							}
						}
					}
				}
			}

			// Destory
			DestroyImmediate(this, false);

		}


		// LGC
#if UNITY_EDITOR
		private void SaveMap (Object map) {
			UnityEditor.EditorUtility.SetDirty(map);
			//UnityEditor.AssetDatabase.SaveAssets();
		}
#else
		private void SaveMap (Object map) {
		
		}
#endif


	}
}