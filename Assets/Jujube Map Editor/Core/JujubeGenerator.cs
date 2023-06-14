namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	public class JujubeGenerator : MonoBehaviour {




		#region --- SUB ---


		public enum LayerType {
			Ground = 0,
			Water = 1,
			Item = 2,
		}


		public enum GroundStyle {
			Mainland = 0,
			Island = 1,
			Basin = 2,

		}


		[System.Flags]
		public enum LocationType {
			None = 0,
			OnGround = 1 << 1,
			InGround = 1 << 2,
			OnWater = 1 << 3,
			InWater = 1 << 4,
		}


		[System.Serializable]
		public class ItemData {

			public List<int> Prefabs => m_Prefabs;
			public LocationType Location { get => m_Location; set => m_Location = value; }
			public int Probability { get => Mathf.Clamp(m_Probability, 0, 100); set => m_Probability = Mathf.Clamp(value, 0, 100); }
			public int MinHeight { get => Mathf.Clamp(m_MinHeight, 0, MAX_SIZE); set => m_MinHeight = Mathf.Clamp(value, 0, MAX_SIZE); }
			public int MaxHeight { get => Mathf.Clamp(m_MaxHeight, 0, MAX_SIZE); set => m_MaxHeight = Mathf.Clamp(value, 0, MAX_SIZE); }
			public bool AllowOverlap { get => m_AllowOverlap; set => m_AllowOverlap = value; }
			public bool RotatePrefab { get => m_RotatePrefab; set => m_RotatePrefab = value; }

			[SerializeField] private List<int> m_Prefabs = new List<int> { 0 };
			[SerializeField] private LocationType m_Location = LocationType.OnGround;
			[SerializeField] private int m_Probability = 25;
			[SerializeField] private int m_MinHeight = 0;
			[SerializeField] private int m_MaxHeight = MAX_SIZE;
			[SerializeField] private bool m_AllowOverlap = false;
			[SerializeField] private bool m_RotatePrefab = false;
#if UNITY_EDITOR
			public bool Editor_OpenedInInspector = false;
#endif
		}


		#endregion




		#region --- VAR ---


		// Const
		private const int MIN_SIZE = 8;
		private const int MAX_SIZE = 128;
		private const float CELL_SIZE_MIN = 0.001f;

		// Api
		public JujubePalette Palette => m_Palette;
		public byte[] Seed => m_Seed;
		public int SizeX { get => Mathf.Clamp(m_SizeX, MIN_SIZE, MAX_SIZE); set => m_SizeX = Mathf.Clamp(value, MIN_SIZE, MAX_SIZE); }
		public int SizeY { get => Mathf.Clamp(m_SizeY, MIN_SIZE, MAX_SIZE); set => m_SizeY = Mathf.Clamp(value, MIN_SIZE, MAX_SIZE); }
		public float CellSize => Mathf.Max(m_CellSize, CELL_SIZE_MIN);
		public JujubePrefabScaleMode PrefabScale => m_PrefabScale;
		public JujubePrefabPivotMode PrefabPivot => m_PrefabPivot;

		public bool UseGround { get => m_UseGround; set => m_UseGround = value; }
		public int Ground_Iteration { get => Mathf.Clamp(m_Ground_Iteration, 1, 16); set => m_Ground_Iteration = Mathf.Clamp(value, 1, 16); }
		public float Ground_IterationRadius { get => Mathf.Clamp(m_Ground_IterationRadius, 1, 16); set => m_Ground_IterationRadius = Mathf.Clamp(value, 1, 16); }
		public int Ground_MinHeight { get => Mathf.Clamp(m_Ground_MinHeight, -MAX_SIZE, MAX_SIZE); set => m_Ground_MinHeight = Mathf.Clamp(value, -MAX_SIZE, MAX_SIZE); }
		public int Ground_MaxHeight { get => Mathf.Clamp(m_Ground_MaxHeight, -MAX_SIZE, MAX_SIZE); set => m_Ground_MaxHeight = Mathf.Clamp(value, -MAX_SIZE, MAX_SIZE); }
		public int Ground_Edge { get => Mathf.Clamp(m_Ground_Edge, 1, MAX_SIZE); set => m_Ground_Edge = Mathf.Clamp(value, 1, MAX_SIZE); }
		public GroundStyle Ground_Style { get => m_Ground_Style; set => m_Ground_Style = value; }
		public bool Ground_RotatePrefab { get => m_Ground_RotatePrefab; set => m_Ground_RotatePrefab = value; }
		public List<int> Ground_Prefabs => m_Ground_Prefabs;

		public bool UseWater { get => m_UseWater; set => m_UseWater = value; }
		public int Water_Height { get => Mathf.Clamp(m_Water_Height, 0, MAX_SIZE); set => m_Water_Height = Mathf.Clamp(value, 0, MAX_SIZE); }
		public bool Water_RotatePrefab { get => m_Water_RotatePrefab; set => m_Water_RotatePrefab = value; }
		public List<int> Water_Prefabs => m_Water_Prefabs;

		public bool UseItem { get => m_UseItem; set => m_UseItem = value; }
		public List<ItemData> Items => m_Items;

		// Ser
		[SerializeField, NullAlert] private JujubePalette m_Palette = null;
		[SerializeField, HideInInspector] private byte[] m_Seed = new byte[0];
		[SerializeField, Clamp(MIN_SIZE, MAX_SIZE)] private int m_SizeX = 64;
		[SerializeField, Clamp(MIN_SIZE, MAX_SIZE)] private int m_SizeY = 64;
		[SerializeField, Clamp(0.001f, float.MaxValue)] private float m_CellSize = 1f;
		[SerializeField] private JujubePrefabScaleMode m_PrefabScale = JujubePrefabScaleMode.LocalScaleAndCellSize;
		[SerializeField] private JujubePrefabPivotMode m_PrefabPivot = JujubePrefabPivotMode.Center;
		[SerializeField] private bool m_LoadAsShell = true;

		[SerializeField] private bool m_UseGround = true;
		[SerializeField] private int m_Ground_Iteration = 4;
		[SerializeField] private float m_Ground_IterationRadius = 4f;
		[SerializeField] private int m_Ground_MinHeight = -1;
		[SerializeField] private int m_Ground_MaxHeight = 8;
		[SerializeField] private int m_Ground_Edge = 5;
		[SerializeField] private GroundStyle m_Ground_Style = GroundStyle.Mainland;
		[SerializeField] private bool m_Ground_RotatePrefab = true;
		[SerializeField] private List<int> m_Ground_Prefabs = new List<int> { 0 };

		[SerializeField] private bool m_UseWater = true;
		[SerializeField] private int m_Water_Height = 1;
		[SerializeField] private bool m_Water_RotatePrefab = true;
		[SerializeField] private List<int> m_Water_Prefabs = new List<int> { 0 };

		[SerializeField] private bool m_UseItem = true;
		[SerializeField] private List<ItemData> m_Items = new List<ItemData>();


		#endregion




		#region --- EDT ---
#if UNITY_EDITOR
		private void Reset () {
			CalculateSeed();
		}
#endif
		#endregion




		#region --- API ---


		public void SetPalette (JujubePalette pal) => m_Palette = pal;


		public void GenerateAndLoad (System.Action<float> progress = null) {
			var map = Generate(progress);
			if (map != null) {
				Load(map);
				DestroyImmediate(map, false);
			}
		}


		public void CalculateSeed () {
			int sizeX = SizeX;
			int sizeY = SizeY;
			int len = sizeX * sizeY;
			m_Seed = new byte[len];
			for (int i = 0; i < len; i++) {
				m_Seed[i] = (byte)Random.Range(byte.MinValue, byte.MaxValue + 1);
			}
		}


		public JujubeMap Generate (System.Action<float> progress = null) {
			if (m_Seed.Length != m_SizeX * m_SizeY) {
				CalculateSeed();
			}
			JujubeMap map = ScriptableObject.CreateInstance<JujubeMap>();
			map.name = $"Generated Map {SizeX}x{SizeY}";
			map.AddLayer("Ground");
			map.AddLayer("Water");
			map.AddLayer("Item");
			try {
				Generate_Ground(ref map, progress);
				Generate_Water(ref map, progress);
				Generate_Item(ref map, progress);
			} catch (System.Exception ex) {
				Debug.LogWarning(ex);
			}
			progress?.Invoke(2f);
			return map;
		}


		public void Load (JujubeMap map) {

			if (map == null) {
				Debug.LogWarning("[Jujube Generator] Can not load map because map is null.");
				return;
			}

			if (Palette == null) {
				Debug.LogWarning("[Jujube Generator] Can not load map because no palette attached.");
				return;
			}

			if (!gameObject.scene.IsValid()) { return; }

			DestroyAllChildrenImmediate(transform);
			float cellSize = CellSize;
			Vector3 pivotOffset = new Vector3(
				0f,
				m_PrefabPivot == JujubePrefabPivotMode.Top ? cellSize / 2f :
				m_PrefabPivot == JujubePrefabPivotMode.Bottom ? -cellSize / 2f :
				0f, 0f
			);
			var groundHash = new HashSet<Vector3Int>();

			// Shell Init
			if (m_LoadAsShell && map.LayerCount > (int)LayerType.Ground) {
				Vector3Int pos;
				var groundLayer = map[(int)LayerType.Ground];
				int blockCount = groundLayer.BlockCount;
				for (int i = 0; i < blockCount; i++) {
					pos = groundLayer[i].Position;
					if (!groundHash.Contains(pos)) {
						groundHash.Add(pos);
					}
				}
			}

			Random.InitState(m_Seed[0] * m_Seed[1]);

			// Load Map
			int layerCount = map.LayerCount;
			Vector3Int _pos;
			var jblockHash = new HashSet<GameObject>();
			for (int i = 0; i < m_Palette.Count; i++) {
				var prefab = m_Palette[i];
				if (prefab != null && !jblockHash.Contains(prefab) && prefab.GetComponent<JBlock>()) {
					jblockHash.Add(prefab);
				}
			}
			for (int i = 0; i < layerCount; i++) {
				bool shellCheck = m_LoadAsShell && (i == (int)LayerType.Ground || i == (int)LayerType.Water);
				// Spawn Layer
				var layer = map[i];
				var blockCount = layer.BlockCount;
				var layerTF = new GameObject(layer.LayerName + " (don't save)").transform;
				layerTF.SetParent(transform);
				layerTF.SetAsLastSibling();
				layerTF.localPosition = Vector3.zero;
				layerTF.localRotation = Quaternion.identity;
				layerTF.localScale = Vector3.one;
				layerTF.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
				for (int j = 0; j < blockCount; j++) {
					var block = layer[j];
					// Shell Check
					if (shellCheck) {
						_pos = block.Position;
						if (
							groundHash.Contains(new Vector3Int(_pos.x - 1, _pos.y, _pos.z)) &&
							groundHash.Contains(new Vector3Int(_pos.x + 1, _pos.y, _pos.z)) &&
							groundHash.Contains(new Vector3Int(_pos.x, _pos.y - 1, _pos.z)) &&
							groundHash.Contains(new Vector3Int(_pos.x, _pos.y + 1, _pos.z)) &&
							groundHash.Contains(new Vector3Int(_pos.x, _pos.y, _pos.z - 1)) &&
							groundHash.Contains(new Vector3Int(_pos.x, _pos.y, _pos.z + 1))
						) {
							continue;
						}
					}
					// Spawn Block
					var prefab = m_Palette[block.Index];
					if (prefab == null) {
						prefab = m_Palette.Failback;
					}
					var blockTF = Instantiate(prefab, layerTF).transform;
					blockTF.name = block.Index.ToString();
					blockTF.SetAsLastSibling();
					blockTF.localPosition = (Vector3)block.Position * cellSize + pivotOffset;
					blockTF.localRotation = Quaternion.Euler(0f, block.RotZ * 90f, 0f);
					blockTF.localScale = GetPrefabLocalScale(prefab.transform);
					blockTF.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
					if (jblockHash.Contains(prefab)) {
						var jblocks = blockTF.GetComponents<JBlock>();
						foreach (var jblock in jblocks) {
							jblock.OnBlockLoaded(null, block);
						}
					}
				}
			}
		}


		#endregion




		#region --- LGC ---


		private void Generate_Ground (ref JujubeMap map, System.Action<float> progress = null) {

			if (map == null || !m_UseGround || m_Ground_Prefabs == null || m_Ground_Prefabs.Count == 0) { return; }

			int groundMin = Mathf.Min(Ground_MinHeight, Ground_MaxHeight);
			int groundMax = Mathf.Max(Ground_MinHeight, Ground_MaxHeight);
			int groundEdge = Ground_Edge;
			int iteration = Ground_Iteration;
			float iterationRadius = Ground_IterationRadius;
			int sizeX = SizeX;
			int sizeY = SizeY;
			int prefabLen = m_Ground_Prefabs.Count;
			float[,] groundHeight = new float[sizeX, sizeY];
			float[,] groundHeightTemp = new float[sizeX, sizeY];

			// Init
			LogProgress(progress, 0, 0.1f);
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					float height = Seed[y * sizeX + x];
					groundHeight[x, y] = height;
					groundHeightTemp[x, y] = height;
				}
			}

			// Ground Type
			if (m_Ground_Style != GroundStyle.Mainland) {
				float edgeValue = m_Ground_Style == GroundStyle.Island ? 0f : 255f;
				for (int x = 0; x < sizeX; x++) {
					for (int e = 0; e < groundEdge && e < (sizeY - 1) / 2; e++) {
						groundHeight[x, e] = edgeValue;
						groundHeightTemp[x, e] = edgeValue;
						groundHeight[x, sizeY - 1 - e] = edgeValue;
						groundHeightTemp[x, sizeY - 1 - e] = edgeValue;
					}
				}
				for (int y = 0; y < sizeY; y++) {
					for (int e = 0; e < groundEdge && e < (sizeX - 1) / 2; e++) {
						groundHeight[e, y] = edgeValue;
						groundHeightTemp[e, y] = edgeValue;
						groundHeight[sizeX - 1 - e, y] = edgeValue;
						groundHeightTemp[sizeX - 1 - e, y] = edgeValue;
					}
				}
			}

			// Iter
			float heightMin = float.MaxValue;
			float heightMax = float.MinValue;
			float _height;
			for (int iter = 0; iter < iteration; iter++) {
				LogProgress(progress, 0, Remap(0, iteration - 1, 0.2f, 0.7f, iter));
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						groundHeightTemp[x, y] = GetIteration(x, y, sizeX, sizeY, iterationRadius, groundHeight);
					}
				}
				var temp = groundHeight;
				groundHeight = groundHeightTemp;
				groundHeightTemp = temp;
			}
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					_height = groundHeight[x, y];
					heightMin = Mathf.Min(heightMin, _height);
					heightMax = Mathf.Max(heightMax, _height);
				}
			}
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					groundHeight[x, y] = Remap(heightMin, heightMax, groundMin, groundMax, groundHeight[x, y]);
				}
			}

			// To Map
			LogProgress(progress, 0, 0.8f);
			var layer = map[(int)LayerType.Ground];
			var blocks = layer.Blocks;
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					int height = Mathf.RoundToInt(groundHeight[x, y]);
					if (height < 0) { continue; }
					for (int z = 0; z < height; z++) {
						var block = new JujubeBlock() {
							Index = m_Ground_Prefabs[Mathf.Clamp(
								Mathf.FloorToInt(Remap(0, groundMax - 1, 0, prefabLen, z)),
								0, prefabLen - 1
							)],
							X = x,
							Y = z,
							Z = y,
							RotZ = m_Ground_RotatePrefab ? m_Seed[y * sizeX + x] % 4 : 0,
						};
						blocks.Add(block);
					}
				}
			}
		}


		private void Generate_Water (ref JujubeMap map, System.Action<float> progress = null) {

			if (map == null || !m_UseWater || m_Water_Prefabs == null || m_Water_Prefabs.Count == 0) { return; }
			LogProgress(progress, 1, 0.5f);

			int sizeX = SizeX;
			int sizeY = SizeY;
			int waterHeight = Water_Height;
			int prefabLen = m_Water_Prefabs.Count;
			var groundHash = new HashSet<Vector3Int>();
			Vector3Int pos;

			// Init Hash
			var groundLayer = map[(int)LayerType.Ground];
			int blockCount = groundLayer.BlockCount;
			for (int i = 0; i < blockCount; i++) {
				pos = groundLayer[i].Position;
				if (!groundHash.Contains(pos)) {
					groundHash.Add(pos);
				}
			}

			// Water
			var waterLayer = map[(int)LayerType.Water];
			for (int x = 0; x < sizeX; x++) {
				for (int y = 0; y < sizeY; y++) {
					for (int z = 0; z < waterHeight; z++) {
						if (groundHash.Contains(new Vector3Int(x, z, y))) { continue; }
						waterLayer.Blocks.Add(new JujubeBlock() {
							Index = m_Water_Prefabs[Mathf.Clamp(
								Mathf.FloorToInt(Remap(0, waterHeight - 1, 0, prefabLen, z)),
								0, prefabLen - 1
							)],
							X = x,
							Y = z,
							Z = y,
							RotZ = m_Water_RotatePrefab ? m_Seed[y * sizeX + x] % 4 : 0,
						});
					}
				}
			}
		}


		private void Generate_Item (ref JujubeMap map, System.Action<float> progress = null) {
			if (map == null || !m_UseItem) { return; }
			LogProgress(progress, 2, 0.5f);

			// 0:none 1:ground, 2:water, 3:ground_item, 4:water_item
			var blockMap = new Dictionary<Vector3Int, int>();
			var heightMap = new Dictionary<Vector2Int, (int min, int max)>();
			int sizeX = SizeX;
			int sizeY = SizeY;
			int min, max;
			var groundLayer = map[(int)LayerType.Ground];
			var waterLayer = map[(int)LayerType.Water];
			var itemLayer = map[(int)LayerType.Item];
			int groundBlockCount = groundLayer.BlockCount;
			int waterBlockCount = waterLayer.BlockCount;
			Vector3Int pos = default;
			Vector2Int pos2 = default;

			// Init
			for (int i = 0; i < groundBlockCount; i++) {
				pos = groundLayer[i].Position;
				AddToBlockMap(1);
			}
			for (int i = 0; i < waterBlockCount; i++) {
				pos = waterLayer[i].Position;
				AddToBlockMap(2);
			}
			void AddToBlockMap (byte _blockID) {
				if (!blockMap.ContainsKey(pos)) {
					blockMap.Add(pos, _blockID);
					pos2 = new Vector2Int(pos.x, pos.z);
					if (heightMap.ContainsKey(pos2)) {
						(min, max) = heightMap[pos2];
						min = Mathf.Min(pos.y, min);
						max = Mathf.Max(pos.y, max);
						heightMap[pos2] = (min, max);
					} else {
						heightMap.Add(pos2, (pos.y, pos.y));
					}
				}
			}

			// Spawn Item
			// 0:none 1:ground, 2:water, 3:ground_item, 4:water_item, 5:item
			int heightMin, heightMax, blockID;
			for (int i = 0; i < Items.Count; i++) {
				var item = Items[i];
				if (item.Probability == 0 || item.Prefabs == null || item.Prefabs.Count == 0) { continue; }
				int prefabCount = item.Prefabs.Count;
				int seedOffset = prefabCount + (int)item.Location + item.MinHeight + item.MaxHeight + i;
				for (int x = 0; x < sizeX; x++) {
					for (int y = 0; y < sizeY; y++) {
						pos2.x = x;
						pos2.y = y;
						if (!heightMap.ContainsKey(pos2)) { continue; }
						(heightMin, heightMax) = heightMap[pos2];
						min = Mathf.Max(heightMin, item.MinHeight);
						max = Mathf.Min(heightMax + 1, item.MaxHeight);
						if (min > max) { continue; }
						pos.x = x;
						pos.z = y;
						byte seed = m_Seed[y * sizeX + x];
						Random.InitState(seed * seed + seedOffset * seedOffset);
						float randomValueA = Random.value;
						float randomValueB = Random.value;
						for (int z = min; z <= max; z++) {
							// Check Probability
							if (item.Probability != 100 && randomValueA * 100f >= item.Probability) { continue; }
							// Check Overlap
							pos.y = z;
							blockID = blockMap.ContainsKey(pos) ? blockMap[pos] : 0;
							if (blockID > 2 && !item.AllowOverlap) { continue; }
							// Check Location
							if (!CheckLocaction(pos, blockID, item.Location, blockMap)) { continue; }
							// Set Item
							if (blockID == 0) {
								blockMap.Add(pos, 5);
							} else if (blockID < 3) {
								blockMap[pos] = blockID + 2;
							}
							itemLayer.Blocks.Add(new JujubeBlock() {
								Index = item.Prefabs[(int)Mathf.Repeat(randomValueB * prefabCount, prefabCount - 0.0001f)],
								X = x,
								Y = z,
								Z = y,
								RotZ = item.RotatePrefab ? seed % 4 : 0,
							});
						}
					}
				}
			}
		}


		#endregion




		#region --- UTL ---


		private void DestroyAllChildrenImmediate (Transform target) {
			int len = target.childCount;
			for (int i = 0; i < len; i++) {
				DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}


		private Vector3 GetPrefabLocalScale (Transform prefab) {
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


		private float Remap (float l, float r, float newL, float newR, float t) {
			return l == r ? l : Mathf.LerpUnclamped(
				newL, newR,
				(t - l) / (r - l)
			);
		}


		private void LogProgress (System.Action<float> action, int step, float progress) => action?.Invoke(Remap(0f, 1f, step / 3f, (step + 1) / 3f, progress));


		private float GetIteration (int x, int y, int sizeX, int sizeY, float radius, float[,] source) {
			int l = Mathf.Max(Mathf.FloorToInt(x - radius), 0);
			int r = Mathf.Min(Mathf.CeilToInt(x + radius), sizeX - 1);
			int d = Mathf.Max(Mathf.FloorToInt(y - radius), 0);
			int u = Mathf.Min(Mathf.CeilToInt(y + radius), sizeY - 1);
			float deltaX, deltaY;
			float result = 0f;
			int count = 0;
			for (int _x = l; _x <= r; _x++) {
				for (int _y = d; _y <= u; _y++) {
					deltaX = Mathf.Abs(x - _x);
					deltaY = Mathf.Abs(y - _y);
					if ((deltaX * deltaX + deltaY * deltaY) > radius * radius) { continue; }
					result += source[_x, _y];
					count++;
				}
			}
			return result / count;
		}


		private bool CheckLocaction (Vector3Int pos, int blockID, LocationType location, Dictionary<Vector3Int, int> blockMap) {

			// On Ground
			if (location.HasFlag(LocationType.OnGround) && (blockID == 0 || blockID == 5)) {
				Vector3Int _pos = pos;
				_pos.y--;
				int blockID_Bottom = blockMap.ContainsKey(_pos) ? blockMap[_pos] : 0;
				if (blockID_Bottom == 1 || blockID_Bottom == 3) {
					return true;
				}
			}

			// In Ground
			if (location.HasFlag(LocationType.InGround) && (blockID == 1 || blockID == 3)) { return true; }

			// On Water
			if (location.HasFlag(LocationType.OnWater) && (blockID == 0 || blockID == 5)) {
				Vector3Int _pos = pos;
				_pos.y--;
				int blockID_Bottom = blockMap.ContainsKey(_pos) ? blockMap[_pos] : 0;
				if (blockID_Bottom == 2 || blockID_Bottom == 4) {
					return true;
				}
			}

			// In Water
			if (location.HasFlag(LocationType.InWater) && (blockID == 2 || blockID == 4)) { return true; }

			return false;
		}


		#endregion




	}
}