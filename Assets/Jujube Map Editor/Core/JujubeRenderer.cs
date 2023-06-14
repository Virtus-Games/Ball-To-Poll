namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public enum JujubeRendererMode {
		Develop = 0,
		Release = 1,
	}



	public enum JujubePrefabScaleMode {
		One = 0,
		LocalScale = 1,
		CellSize = 2,
		LocalScaleAndCellSize = 3,
	}


	public enum JujubePrefabPivotMode {
		Top = 0,
		Center = 1,
		Bottom = 2,
	}



	[AddComponentMenu("Jujube/Jujube Renderer")]
	public class JujubeRenderer : MonoBehaviour {




		#region --- VAR ---


		// Const
		private const float CELL_SIZE_MIN = 0.001f;

		// Api
		public JujubeRendererMode Mode {
			get => m_Mode; set => m_Mode = value;
		}
		public bool Editable => Map != null && Palette != null;
		public JujubeMap Map => m_Map;
		public JujubePalette Palette => m_Palette;
		public JujubePrefabScaleMode PrefabScale => m_PrefabScale;
		public JujubePrefabPivotMode PrefabPivot => m_PrefabPivot;
		public float CellSize => Mathf.Max(m_CellSize, CELL_SIZE_MIN);

		// Ser
		[NullAlert, SerializeField] protected JujubeMap m_Map = null;
		[NullAlert, SerializeField] protected JujubePalette m_Palette = null;
		[SerializeField] protected float m_CellSize = 1f;
		[SerializeField] protected JujubePrefabScaleMode m_PrefabScale = JujubePrefabScaleMode.LocalScaleAndCellSize;
		[SerializeField] protected JujubePrefabPivotMode m_PrefabPivot = JujubePrefabPivotMode.Center;
		[SerializeField] private JujubeRendererMode m_Mode = JujubeRendererMode.Develop;

		// Data
		private List<BoxCollider> EditorColCache = new List<BoxCollider>();
		private static readonly Collider[] ColCache = new Collider[16];


		#endregion




		#region --- API ---


		// Data
		public void SetMap (JujubeMap map) => m_Map = map;


		public void SetPalette (JujubePalette pal) => m_Palette = pal;


		// Map
		public void ReloadAll (bool forceRespawn = false) {

			// Spawn Map
			if (m_Map == null || m_Palette == null || !gameObject.scene.IsValid()) { return; }

			// Layers
			if (forceRespawn) {
				DestroyAllChirldrenImmediate(transform);
			}
			for (int i = 0; i < m_Map.Layers.Count; i++) {
				var layer = m_Map.Layers[i];
				var layerTF = SpawnLayerTF(layer, i);
				int blockCount = layer.Blocks.Count;
				for (int blockIndex = 0; blockIndex < blockCount; blockIndex++) {
					SpawnBlockTF(layer.Blocks[blockIndex], layerTF, blockIndex, forceRespawn);
				}
			}

		}


		// Get
		public Transform GetLayerTF (int index) => index >= 0 && index < transform.childCount ? transform.GetChild(index) : null;


		public Transform GetBlockTF (int layerIndex, int blockIndex) {
			var layerTf = GetLayerTF(layerIndex);
			if (layerTf != null) {
				return blockIndex >= 0 && blockIndex < layerTf.childCount ? layerTf.GetChild(blockIndex) : null;
			}
			return null;
		}


		public (JujubeBlock block, Transform blockTF) GetBlockAndBlockTF (int layerIndex, int blockIndex) {
			if (layerIndex < 0 || layerIndex >= Map.LayerCount) { return (null, null); }
			var layer = Map[layerIndex];
			if (layer == null || blockIndex < 0 || blockIndex >= layer.BlockCount) { return (null, null); }
			var layerTf = GetLayerTF(layerIndex);
			if (layerTf != null) {
				var blockTF = blockIndex >= 0 && blockIndex < layerTf.childCount ? layerTf.GetChild(blockIndex) : null;
				if (blockTF != null) {
					var block = layer.Blocks[blockIndex];
					if (block != null) {
						return (block, blockTF);
					}
				}
			}
			return (null, null);
		}


		public bool GetLayerVisible (int index) {
			var layerTF = GetLayerTF(index);
			return layerTF && layerTF.gameObject.activeSelf;
		}


		public void SetLayerVisible (int index, bool visible) {
			var layerTF = GetLayerTF(index);
			if (layerTF) {
				layerTF.gameObject.SetActive(visible);
			}
			if (m_Map != null && index >= 0 && index < m_Map.LayerCount) {
				m_Map[index].Visible = visible;
			}
		}


		public (int layer, int blockIndex, JujubeBlock block) GetBlockIndex (Vector3Int pos, int layerIndex, bool visibleLayerOnly) {
			if (pos.y < 0) { return (-1, -1, null); }
			if (m_Mode == JujubeRendererMode.Develop) {
				int len = Physics.OverlapSphereNonAlloc(
					transform.localToWorldMatrix.MultiplyPoint3x4((Vector3)pos * m_CellSize),
					m_CellSize * 0.1f,
					ColCache,
					Physics.AllLayers,
					QueryTriggerInteraction.Ignore
				);
				for (int i = 0; i < len; i++) {
					var col = ColCache[i];
					if (!IsBlockTF(col.transform)) { continue; }
					int colLayerIndex = col.transform.parent.GetSiblingIndex();
					if (visibleLayerOnly && !col.transform.parent.gameObject.activeSelf) { continue; }
					if (layerIndex < 0 || layerIndex == colLayerIndex) {
						int colBlockIndex = col.transform.GetSiblingIndex();
						var layer = m_Map[colLayerIndex];
						if (colBlockIndex < layer.BlockCount) {
							return (colLayerIndex, colBlockIndex, layer[colBlockIndex]);
						}
						break;
					}
				}
			} else {
				if (layerIndex < 0) {
					// All Layer
					int layerCount = m_Map.Layers.Count;
					for (int index = 0; index < layerCount; index++) {
						var layer = m_Map.Layers[index];
						if (layer == null) { continue; }
						if (visibleLayerOnly && !layer.Visible) { continue; }
						// Get Without Cache
						int blockCount = layer.Blocks.Count;
						for (int i = 0; i < blockCount; i++) {
							var block = layer.Blocks[i];
							if (block != null && block.Position == pos) {
								return (index, i, block);
							}
						}
					}
				} else {
					var layer = m_Map[layerIndex];
					if (layer == null) { return (-1, -1, null); }
					if (visibleLayerOnly && !layer.Visible) { return (-1, -1, null); }
					// Get Without Cache
					int blockCount = layer.Blocks.Count;
					for (int i = 0; i < blockCount; i++) {
						var block = layer.Blocks[i];
						if (block != null && block.Position == pos) {
							return (layerIndex, i, block);
						}
					}
				}
			}
			return (-1, -1, null);
		}


		// Set
		public void AddLayer () {
			m_Map.AddLayer();
			int layerIndex = m_Map.LayerCount - 1;
			SpawnLayerTF(m_Map.Layers[layerIndex], layerIndex);
		}


		public JujubeBlock AddBlock (int layerIndex, int paletteIndex, Vector3Int pos, int rotZ) => AddBlock(m_Map[layerIndex], layerIndex, paletteIndex, pos, rotZ);


		public JujubeBlock AddBlock (JujubeLayer layer, int layerIndex, int paletteIndex, Vector3Int pos, int rotZ) {
			var block = new JujubeBlock() {
				X = pos.x,
				Y = pos.y,
				Z = pos.z,
				Index = paletteIndex,
				RotZ = rotZ,
			};
			layer.Blocks.Add(block);
			var blockTF = SpawnBlockTF(block, GetLayerTF(layerIndex), layer.Blocks.Count - 1, true);
			RespawnEditColliderForBlock(blockTF);
			return block;
		}


		// Delete
		public void RemoveBlock (int layerIndex, int blockIndex) {
			if (m_Map == null) { return; }
			var layer = m_Map[layerIndex];
			if (layer == null) { return; }
			var blockTF = GetBlockTF(layerIndex, blockIndex);
			if (blockTF != null && blockIndex >= 0 && blockIndex < layer.BlockCount) {
				layer.Blocks.RemoveAt(blockIndex);
				DestroyImmediate(blockTF.gameObject, false);
			}
		}


		public void RemoveBlock (int layerIndex, JujubeBlock block) {
			if (m_Map == null) { return; }
			var layer = m_Map[layerIndex];
			if (layer == null) { return; }
			int blockIndex = layer.Blocks.IndexOf(block);
			if (blockIndex >= 0) {
				var blockTF = GetBlockTF(layerIndex, blockIndex);
				if (blockTF != null && blockIndex >= 0 && blockIndex < layer.BlockCount) {
					layer.Blocks.RemoveAt(blockIndex);
					DestroyImmediate(blockTF.gameObject, false);
				}
			}
		}


		public void RespawnBlock (int layerIndex, int blockIndex, JujubeBlock block) {
			if (m_Map == null || block == null) { return; }
			var layer = m_Map[layerIndex];
			if (layer == null) { return; }
			var blockTF = GetBlockTF(layerIndex, blockIndex);
			if (blockTF != null && blockIndex >= 0 && blockIndex < layer.BlockCount) {
				var layerTF = blockTF.parent;
				DestroyImmediate(blockTF.gameObject, false);
				var newBlockTF = SpawnBlockTF(block, layerTF, blockIndex, true);
				RespawnEditColliderForBlock(newBlockTF);
			}
		}


		// Check
		public bool IsBlockTF (Transform blockTF) => blockTF && blockTF.parent && blockTF.parent.parent && blockTF.parent.parent == transform;


		public bool IsJblock (Transform blockTF) => blockTF.name.Length > 0 && blockTF.name[0] == 'J';


		// Prefab
		public Vector3 GetPrefabLocalScale (Transform prefab) {
			if (prefab == null) { return Vector3.one; }
			switch (m_PrefabScale) {
				default:
					return Vector3.one;
				case JujubePrefabScaleMode.LocalScale:
					return prefab.localScale;
				case JujubePrefabScaleMode.CellSize:
					return Vector3.one * CellSize;
				case JujubePrefabScaleMode.LocalScaleAndCellSize:
					return prefab.localScale * CellSize;
			}
		}


		#endregion




		#region --- LGC ---


		// Spawn
		private Transform SpawnLayerTF (JujubeLayer layer, int layerIndex) {
			Transform layerTF;
			if (layerIndex >= 0 && layerIndex < transform.childCount) {
				layerTF = transform.GetChild(layerIndex);
			} else {
				layerTF = new GameObject(layer.LayerName).transform;
				layerTF.SetParent(transform);
				layerTF.SetAsLastSibling();
			}
			layerTF.localPosition = Vector3.zero;
			layerTF.localRotation = Quaternion.identity;
			layerTF.localScale = Vector3.one;
			layerTF.gameObject.SetActive(layer.Visible);
			layerTF.gameObject.hideFlags = GetItemFlag(m_Mode);
			return layerTF;
		}


		private Transform SpawnBlockTF (JujubeBlock block, Transform layerTF, int index, bool forceRespawn) {
			Transform blockTF = null;
			GameObject prefab = null;
			int prefabIndex = -1;
			int blockTFCount = layerTF.childCount;
			if (block != null) {
				prefabIndex = block.Index;
				prefab = m_Palette[prefabIndex];
				if (prefab == null) {
					prefab = m_Palette.Failback;
				}
			}
			string blockNameB = "B" + prefabIndex.ToString();
			string blockNameJ = "J" + prefabIndex.ToString();
			bool destroy = forceRespawn;
			bool newblock = false;

			if (!forceRespawn && index >= 0 && index < blockTFCount) {
				blockTF = layerTF.GetChild(index);
				if (blockTF.name != blockNameB && blockTF.name != blockNameJ) {
					destroy = true;
				}
			}

			if (blockTF != null && destroy) {
				DestroyImmediate(blockTF.gameObject, false);
				blockTF = null;
			}

			if (blockTF == null && prefab != null) {
				blockTF = Instantiate(prefab.gameObject, layerTF).transform;
				newblock = true;
			}

			if (blockTF == null) {
				blockTF = new GameObject(blockNameB).transform;
				blockTF.SetParent(layerTF);
			}

			if (!blockTF.gameObject.activeSelf) {
				blockTF.gameObject.SetActive(true);
			}

			float cellSize = CellSize;
			Vector3 pivotOffset = new Vector3(
				0f,
				m_PrefabPivot == JujubePrefabPivotMode.Top ? cellSize / 2f :
				m_PrefabPivot == JujubePrefabPivotMode.Bottom ? -cellSize / 2f :
				0f, 0f
			);

			// Spawn
			blockTF.localPosition = block != null ? (Vector3)block.Position * cellSize + pivotOffset : Vector3.zero;
			blockTF.localRotation = Quaternion.Euler(0f, block != null ? block.RotZ * 90f : 0f, 0f);
			blockTF.localScale = GetPrefabLocalScale(prefab != null ? prefab.transform : null);
			blockTF.gameObject.hideFlags = GetItemFlag(m_Mode);
			if (index >= 0) {
				blockTF.SetSiblingIndex(index);
			} else {
				blockTF.SetAsLastSibling();
			}

			// New Block
			if (newblock) {
				var jblock = blockTF.GetComponent<JBlock>();
				if (jblock != null) {
					blockTF.name = blockNameJ;
					//jblock.OnBlockLoaded(this, block);
					// More JBlock
					var jBlocks = blockTF.GetComponents<JBlock>();
					if (jBlocks.Length > 0) {
						foreach (var _block in jBlocks) {
							_block.OnBlockLoaded(this, block);
						}
					}
				} else {
					blockTF.name = blockNameB;
				}
				// Editor Col
				RespawnEditColliderForBlock(blockTF);
			}

			return blockTF;
		}


		// Edit Collider
		private void RespawnEditColliderForBlock (Transform blockTF) {
			if (m_Map == null || m_Mode != JujubeRendererMode.Develop) { return; }

			// Remove Old Collider
			blockTF.GetComponentsInChildren(false, EditorColCache);
			int len = EditorColCache.Count;
			for (int i = 0; i < len; i++) {
				DestroyImmediate(EditorColCache[0], false);
			}

			// Fix Collider Size
			var box = blockTF.GetComponent<BoxCollider>();
			if (box == null) {
				box = blockTF.gameObject.AddComponent<BoxCollider>();
			}
			var scl = blockTF.localScale;
			float cellSize = CellSize;
			box.size = new Vector3(
				cellSize / (!Mathf.Approximately(scl.x, 0f) ? scl.x : 0.0001f),
				cellSize / (!Mathf.Approximately(scl.y, 0f) ? scl.y : 0.0001f),
				cellSize / (!Mathf.Approximately(scl.z, 0f) ? scl.z : 0.0001f)
			);
			box.center = new Vector3(
				0f,
				PrefabPivot == JujubePrefabPivotMode.Top ? -box.size.y / 2f :
				PrefabPivot == JujubePrefabPivotMode.Bottom ? box.size.y / 2f :
				0f, 0f
			);
		}


		#endregion




		#region --- UTL ---


		private HideFlags GetItemFlag (JujubeRendererMode mode) => mode == JujubeRendererMode.Develop ? HideFlags.NotEditable : HideFlags.None;


		private void DestroyAllChirldrenImmediate (Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}


		#endregion





	}
}