namespace JujubeMapEditor.Core {
	using JetBrains.Annotations;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;




	[System.Serializable]
	public class JujubeBlock {

		// Api
		public Vector3Int Position {
			get => new Vector3Int(X, Y, Z);
			set {
				X = value.x;
				Y = value.y;
				Z = value.z;
			}
		}

		public int Index {
			get => m_Index;
			set => m_Index = value;
		}
		public int X {
			get => m_X;
			set => m_X = (short)Mathf.Clamp(value, short.MinValue, short.MaxValue);
		}
		public int Y {
			get => m_Y;
			set => m_Y = (ushort)Mathf.Clamp(value, ushort.MinValue, ushort.MaxValue);
		}
		public int Z {
			get => m_Z;
			set => m_Z = (short)Mathf.Clamp(value, short.MinValue, short.MaxValue);
		}
		public int RotZ {
			get => m_RotZ;
			set => m_RotZ = (byte)Mathf.Clamp(value, 0, 3);
		}
		public string Data {
			get => m_Data;
			set => m_Data = value;
		}

		// Api-Ser
		[SerializeField] private short m_X = 0;
		[SerializeField] private ushort m_Y = 0;
		[SerializeField] private short m_Z = 0;
		[SerializeField] private byte m_RotZ = 0;
		[SerializeField] private int m_Index = 0;
		[SerializeField] private string m_Data = string.Empty;

	}



	[System.Serializable]
	public class JujubeLayer {

		// Api
		public JujubeBlock this[int index] => Blocks[index];
		public int BlockCount => Blocks.Count;

		// Ser
		public string LayerName = "Layer";
		public bool Visible = true;
		public List<JujubeBlock> Blocks = new List<JujubeBlock>();


	}



	[CreateAssetMenu(menuName = "Jujube Map", fileName = "New Map", order = 203)]
	public class JujubeMap : ScriptableObject {




		#region --- VAR ---


		// Api
		public JujubeLayer this[int index] => index >= 0 && index < m_Layers.Count ? m_Layers[index] : null;

		public JujubeBlock this[int blockIndex, int layerIndex] {
			get {
				var layer = this[layerIndex];
				if (layer == null) { return null; }
				return blockIndex >= 0 && blockIndex < layer.BlockCount ?
					layer[blockIndex] : null;
			}
		}

		public List<JujubeLayer> Layers => m_Layers;
		public int LayerCount => m_Layers.Count;


		// Ser
		[SerializeField] private List<JujubeLayer> m_Layers = new List<JujubeLayer>();


		#endregion




		#region --- API ---



		// API
		public Bounds GetMapBounds (float cellSize) {
			Bounds bounds = new Bounds(Vector3.zero, Vector3.one * cellSize);
			Bounds cellBounds = new Bounds(Vector3.zero, Vector3.one * cellSize);
			int layerCount = LayerCount;
			for (int i = 0; i < layerCount; i++) {
				var layer = this[i];
				int itemCount = layer.BlockCount;
				for (int j = 0; j < itemCount; j++) {
					var item = layer[j];
					cellBounds.center = (Vector3)item.Position * cellSize;
					bounds.Encapsulate(cellBounds);
				}
			}
			return bounds;
		}


		public int GetBlockCount () {
			int count = 0;
			foreach (var layer in m_Layers) {
				count += layer.BlockCount;
			}
			return count;
		}


		// Layer
		public JujubeLayer AddLayer (string layerName = "New Layer") {
			var layer = new JujubeLayer() {
				LayerName = layerName,
				Blocks = new List<JujubeBlock>(),
			};
			m_Layers.Add(layer);
			return layer;
		}

		public void RemoveLayer (int index) {
			if (index >= 0 && index < LayerCount) {
				m_Layers.RemoveAt(index);
			}
		}


		public void SwipeLayer (int index, int altIndex) {
			if (index >= 0 && index < LayerCount && altIndex >= 0 && altIndex < LayerCount) {
				var temp = m_Layers[index];
				m_Layers[index] = m_Layers[altIndex];
				m_Layers[altIndex] = temp;
			}
		}


		public void MergeLayer (int from, int to) {
			if (from < 0 || from >= LayerCount || to < 0 || to >= LayerCount) { return; }
			var layerFrom = m_Layers[from];
			var layerTo = m_Layers[to];
			if (layerFrom == null || layerTo == null) { return; }
			foreach (var block in layerFrom.Blocks) {
				if (block == null) { continue; }
				layerTo.Blocks.Add(block);
			}
			m_Layers.RemoveAt(from);
		}


		public void RemoveOverlappedBlocks (int index) {
			if (index >= LayerCount) { return; }
			int startIndex = index;
			int endIndex = index;
			bool forAllLayers = index < 0;
			if (forAllLayers) {
				// All Layers
				startIndex = 0;
				endIndex = LayerCount - 1;
			}
			// Remove Overlap
			var hash = new HashSet<Vector3Int>();
			for (int layerIndex = startIndex; layerIndex <= endIndex; layerIndex++) {
				var layer = this[layerIndex];
				if (layer == null) { continue; }
				if (!forAllLayers) { hash.Clear(); }
				for (int i = 0; i < layer.BlockCount; i++) {
					var block = layer[i];
					if (block == null || hash.Contains(block.Position)) {
						layer.Blocks.RemoveAt(i);
						i--;
					} else {
						hash.Add(block.Position);
					}
				}
			}
		}


		// Palette Index Fix
		public GameObject[][] GetBlockPrefabs (JujubePalette palette) {
			if (palette == null) { return null; }
			int palCount = palette.Count;
			var result = new GameObject[Layers.Count][];
			for (int i = 0; i < Layers.Count; i++) {
				var layer = this[i];
				result[i] = new GameObject[layer.BlockCount];
				for (int j = 0; j < layer.BlockCount; j++) {
					int index = layer[j].Index;
					result[i][j] = index >= 0 && index < palCount ? palette[index] : null;
				}
			}
			return result;
		}


		public void SetBlockPrefabIndexs (GameObject[][] prefabss, JujubePalette palette) {
			if (palette == null || prefabss == null) { return; }

			// Get Map
			var map = new Dictionary<GameObject, int>();
			for (int i = 0; i < palette.Prefabs.Count; i++) {
				var prefab = palette.Prefabs[i];
				if (prefab == null || map.ContainsKey(prefab)) { continue; }
				map.Add(prefab, i);
			}

			// Set Indexs
			for (int i = 0; i < Layers.Count && i < prefabss.Length; i++) {
				var layer = this[i];
				var prefabs = prefabss[i];
				for (int j = 0; j < layer.BlockCount && j < prefabs.Length; j++) {
					var prefab = prefabs[j];
					if (prefab == null) { continue; }
					var block = layer[j];
					if (block == null) { continue; }
					block.Index = map.ContainsKey(prefab) ? map[prefab] : -1;
				}
			}
		}


		#endregion




	}
}