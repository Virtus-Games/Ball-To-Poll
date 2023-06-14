namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using System.Text;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;


	public class JujubeCombinerWindow : MoenenEditorWindow {





		#region --- SUB ---


		private class ConfigItem {
			public long ChangedTime = 0;
			public bool CombineMesh = true;

		}


		public enum CombineMode {
			OneMeshForWholeMap = 0,
			OneMeshForEachLayer = 1,

		}


		[System.Flags]
		private enum BlockOverlap {
			None = 0,
			All = L | R | D | U | B | F,
			L = 1 << 1,
			R = 1 << 2,
			D = 1 << 3,
			U = 1 << 4,
			B = 1 << 5,
			F = 1 << 6,
		}


		#endregion




		#region --- VAR ---


		// Const
		private const string JUJUBE_MENU_ROOT = "Tools/Jujube Map Editor";
		private const float FIELD_WIDTH = 64;
		private const float FIELD_HEIGHT = 18;
		private const int MAX_CONFIG_DATA_COUNT = 1024;
		private const string PROGRESS_TITLE = "Combine Mesh";

		// Api
		public JujubeMap Map { get; set; } = null;
		public JujubePalette Palette { get; set; } = null;
		public JujubePrefabPivotMode PrefabPivot { get; set; } = JujubePrefabPivotMode.Center;
		public JujubePrefabScaleMode PrefabScale { get; set; } = JujubePrefabScaleMode.LocalScaleAndCellSize;
		public float CellSize { get; set; } = 1f;

		// Short
		private string ConfigPath => EditorUtil.CombinePaths(EditorUtil.GetRootPath(), "Editor", "Data", "Mesh Combiner Config.txt");
		private string ShaderTextureKeyword => !string.IsNullOrEmpty(TextureKeyword.Value) ? TextureKeyword.Value : "_MainTex";
		private Shader PrefabShader {
			get {
				var shader = Shader.Find(PrefabShaderName.Value);
				if (shader == null) {
					shader = Shader.Find("Standard");
				}
				return shader;
			}
			set {
				PrefabShaderName.Value = value != null ? value.name : "Standard";
			}
		}
		private Vector3 PivotOffset => new Vector3(
			0f,
			PrefabPivot == JujubePrefabPivotMode.Top ? CellSize / 2f :
			PrefabPivot == JujubePrefabPivotMode.Bottom ? -CellSize / 2f :
			0f,
			0f
		);

		// Data
		private readonly Dictionary<string, ConfigItem> ConfigMap = new Dictionary<string, ConfigItem>();
		private Vector2 ScrollPosition = default;
		private Texture2D PrefabIcon = null;
		private readonly List<int> MaterialPropertyIdCache = new List<int>();
		private readonly string DONT_COMBINE_NAME = ((int)CombineElementMode.DoNotCombine).ToString();
		private readonly string OVERLAP_NEARBY_NAME = ((int)CombineElementMode.Combine).ToString();

		// Saving
		private EditorSavingString PrefabShaderName = new EditorSavingString("JujubeCombinerWindow.PrefabShaderName", "Standard");
		private EditorSavingString TextureKeyword = new EditorSavingString("JujubeCombinerWindow.TextureKeyword", "_MainTex");
		private EditorSavingInt CombineModeIndex = new EditorSavingInt("JujubeCombinerWindow.CombineModeIndex", 0);
		private EditorSavingBool IgnoreOverlappingTriangle = new EditorSavingBool("JujubeCombinerWindow.IgnoreOverlappingTriangle", true);


		#endregion




		#region --- MSG ---


		[MenuItem(JUJUBE_MENU_ROOT + "/Mesh Combiner", priority = 15)]
		public static void OpenWindow () => OpenCombinerWindow();


		private void OnEnable () {
			PrefabIcon = EditorUtil.GetImage("Prefab Icon.png");
			LoadConfig();
		}


		private void OnGUI () {
			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);
			LayoutV(() => {
				GUI_Config();
				Space(8);
				if (Map != null && Palette != null) {
					// Config
					GUI_LayerConfig();
				} else {
					// Warning
					Space(4);
					EditorGUILayout.HelpBox("No map or palette attached.", MessageType.Warning, true);
				}
			}, false, new GUIStyle() { padding = new RectOffset(24, 24, 12, 12) });
			EditorGUILayout.EndScrollView();

			// Button 
			GUI_ControlButton();
			Space(6);

			// Final
			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl("");
				Repaint();
			}
		}


		private void GUI_Config () {
			Map = EditorGUILayout.ObjectField("Map", Map, typeof(JujubeMap), false) as JujubeMap;
			Palette = EditorGUILayout.ObjectField("Palette", Palette, typeof(JujubePalette), false) as JujubePalette;
			PrefabShader = EditorGUILayout.ObjectField("Override Shader", PrefabShader, typeof(Shader), false) as Shader;
			if (PrefabShader != null) {
				LayoutH(() => {
					EditorGUILayout.PrefixLabel("Texture Keyword");
					if (EditorGUILayout.DropdownButton(new GUIContent(TextureKeyword.Value), FocusType.Passive)) {
						ShowShaderPropertyDropdown();
					}
				});
			}
			CombineModeIndex.Value = (int)(CombineMode)EditorGUILayout.EnumPopup(new GUIContent("Combine Mode"), (CombineMode)CombineModeIndex.Value);
			IgnoreOverlappingTriangle.Value = EditorGUILayout.Toggle(new GUIContent("Ignore Overlap Face"), IgnoreOverlappingTriangle.Value);
		}


		private void GUI_LayerConfig () {
			const int ITEM_WIDTH = 64;
			const int ITEM_HEIGHT = 18;
			EditorGUI.BeginChangeCheck();
			Space(2);

			// Title
			LayoutH(() => {
				// Label
				Space(ITEM_HEIGHT);
				// Layer Name
				GUI.Label(GUIRect(0, ITEM_HEIGHT), "Layer Name", EditorStyles.centeredGreyMiniLabel);
				// Combine Mesh
				GUI.Label(GUIRect(ITEM_WIDTH, ITEM_HEIGHT), "Combine", EditorStyles.centeredGreyMiniLabel);

			});

			// Layers
			int layerCount = Map.LayerCount;
			for (int i = 0; i < layerCount; i++) {
				var layer = Map[i];
				ConfigItem configItem = null;
				if (ConfigMap.ContainsKey(layer.LayerName)) {
					configItem = ConfigMap[layer.LayerName];
				} else {
					ConfigMap.Add(layer.LayerName, configItem = new ConfigItem());
					configItem.ChangedTime = EditorUtil.GetLongTime();
				}
				LayoutH(() => {

					// Index
					GUI.Label(GUIRect(ITEM_HEIGHT, ITEM_HEIGHT), $"{i}.");
					Space(2);

					// Name
					bool oldE = GUI.enabled;
					GUI.enabled = false;
					GUI.Label(GUIRect(0, ITEM_HEIGHT), layer.LayerName, GUI.skin.textField);
					GUI.enabled = oldE;
					Space(2);

					// Combine Mesh
					LayoutH(() => {
						Space((ITEM_WIDTH - ITEM_HEIGHT) / 2f);
						configItem.CombineMesh = EditorGUI.Toggle(GUIRect(ITEM_HEIGHT, ITEM_HEIGHT), configItem.CombineMesh);
						Space((ITEM_WIDTH - ITEM_HEIGHT) / 2f);
					}, false, new GUIStyle() { fixedWidth = ITEM_WIDTH, });

				});
				Space(2);
			}
			if (EditorGUI.EndChangeCheck()) {
				for (int i = 0; i < layerCount; i++) {
					var layer = Map[i];
					if (ConfigMap.ContainsKey(layer.LayerName)) {
						ConfigMap[layer.LayerName].ChangedTime = EditorUtil.GetLongTime();
					}
				}
				SaveConfig();
			}
		}


		private void GUI_ControlButton () {
			LayoutH(() => {
				const int BUTTON_WIDTH = 92;
				const int BUTTON_HEIGHT = 24;
				GUIRect(0, BUTTON_HEIGHT);

				// Cancel
				var rect = GUIRect(BUTTON_WIDTH, BUTTON_HEIGHT);
				if (GUI.Button(rect, "Cancel")) {
					SaveConfig();
					Close();
				}
				Space(2);

				// Export
				if (GUI.Button(GUIRect(BUTTON_WIDTH, BUTTON_HEIGHT), new GUIContent(" Export ", PrefabIcon))) {
					EditorApplication.delayCall += () => {
						string path = EditorUtility.SaveFilePanel("Export Prefab", "Assets", "New Map Prefab", "prefab"); ;
						if (!string.IsNullOrEmpty(path)) {
							ExportCombinedPrefab(EditorUtil.FixedRelativePath(path));
							SaveConfig();
							Close();
						}
					};
				}
				Space(6);
			});
		}


		#endregion




		#region --- API ---


		public static JujubeCombinerWindow OpenCombinerWindow () {
			var window = GetWindow<JujubeCombinerWindow>(true, "Jujube Combiner", true);
			window.minSize = new Vector2(390f, 280f);
			window.maxSize = new Vector2(480f, 360f);
			return window;
		}


		public void ExportCombinedPrefab (string path) {

			// Check
			if (!Combine_Check()) { return; }

			// Make Prefab
			EditorUtil.ProgressBar(PROGRESS_TITLE, "Spawn Prefab", 0.1f);
			Combine_SpawnPrefab(EditorUtil.GetNameWithoutExtension(path), out var rootTF, out var dontCombineRoot, out ConfigItem[] configItems);
			if (rootTF == null) { return; }

			try {

				// Move Out Dont Combine Blocks
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Move Out Don't Combine Blocks", 0.2f);
				Combine_MoveOutDontCombineBlocks(rootTF, dontCombineRoot);

				// Ignore Overlap
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Ignore Overlap", 0.3f);
				Combine_IgnoreOverlap(rootTF, configItems);

				// Find Mesh Filters 
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Find Mesh Filters", 0.4f);
				Combine_FindMeshFilters(rootTF, configItems, out var meshFilterMap);

				// Pack Texture
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Pack Texture", 0.5f);
				Combine_PackTexture(rootTF.name, meshFilterMap, out var texture, out var uvRemap);

				// Combine Meshs
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Combine Meshs", 0.6f);
				Combine_CombineMeshs(rootTF, meshFilterMap, texture, uvRemap, out var material, out var meshs);

				// Clear Root TF
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Clear Transforms", 0.7f);
				Combine_ClearRootTF(rootTF, dontCombineRoot, configItems);

				// Save Prefab
				EditorUtil.ProgressBar(PROGRESS_TITLE, "Save Prefab", 0.8f);
				Combine_SavePrefab(path, rootTF, material, texture, meshs);

			} catch (System.Exception ex) { Debug.LogError(ex); }

			// Final
			EditorUtil.ClearProgressBar();
			DestroyImmediate(rootTF.gameObject, false);
			if (dontCombineRoot != null && dontCombineRoot.childCount == 0) {
				DestroyImmediate(dontCombineRoot.gameObject, false);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

		}


		#endregion




		#region --- LGC ---


		// Combine Pipeline
		private bool Combine_Check () {
			if (PrefabShader == null) {
				Debug.LogWarning("[Jujube Combiner] No shader attached.");
				return false;
			}
			if (PrefabShader.FindPropertyIndex(ShaderTextureKeyword) < 0) {
				Debug.LogWarning("[Jujube Combiner] Texture keyword not found in the shader.");
				return false;
			}
			if (Map == null || Palette == null) {
				Debug.LogWarning("[Jujube Combiner] No map or palette attached.");
				return false;
			}
			return true;
		}


		private void Combine_SpawnPrefab (string rootName, out Transform rootTF, out Transform dontCombineRoot, out ConfigItem[] configItems) {
			var palette = Palette;
			var map = Map;
			int layerCount = map.LayerCount;
			configItems = new ConfigItem[layerCount];
			// JBlock Hash
			var jblockHash = new HashSet<GameObject>();
			for (int i = 0; i < palette.Count; i++) {
				var palPrefab = palette[i];
				if (palPrefab != null && !jblockHash.Contains(palPrefab) && palPrefab.GetComponent<JBlock>()) {
					jblockHash.Add(palPrefab);
				}
			}
			// Root
			rootTF = new GameObject(rootName).transform;
			dontCombineRoot = new GameObject("Dont Combine This Block").transform;
			try {
				rootTF.localPosition = Vector3.zero;
				rootTF.localRotation = Quaternion.identity;
				rootTF.localScale = Vector3.one;
				dontCombineRoot.localPosition = Vector3.zero;
				dontCombineRoot.localRotation = Quaternion.identity;
				dontCombineRoot.localScale = Vector3.one;
				float cellSize = CellSize;
				Vector3 pivotOffset = PivotOffset;
				for (int i = 0; i < layerCount; i++) {
					var layer = map[i];
					var layerTF = new GameObject(layer.LayerName).transform;
					layerTF.SetParent(rootTF);
					layerTF.SetAsLastSibling();
					layerTF.localPosition = Vector3.zero;
					layerTF.localRotation = Quaternion.identity;
					layerTF.localScale = Vector3.one;
					// Config Item
					if (ConfigMap.ContainsKey(layer.LayerName)) {
						configItems[i] = ConfigMap[layer.LayerName];
					} else {
						ConfigMap.Add(layer.LayerName, configItems[i] = new ConfigItem());
						configItems[i].ChangedTime = EditorUtil.GetLongTime();
					}
					bool combine = configItems[i].CombineMesh;
					// Blocks
					var blockCount = layer.BlockCount;
					for (int j = 0; j < blockCount; j++) {
						var block = layer[j];
						// Spawn Block
						var palPrefab = palette[block.Index];
						if (palPrefab == null) {
							palPrefab = palette.Failback;
						}
						var blockTF = Instantiate(palPrefab, layerTF).transform;
						var cElement = blockTF.GetComponent<CombineElement>();
						blockTF.name = cElement != null ? ((int)cElement.CombineMode).ToString() : OVERLAP_NEARBY_NAME;
						blockTF.SetAsLastSibling();
						blockTF.localPosition = (Vector3)block.Position * cellSize + pivotOffset;
						blockTF.localRotation = Quaternion.Euler(0f, block.RotZ * 90f, 0f);
						blockTF.localScale = GetPrefabLocalScale(palPrefab.transform);
						if (cElement != null) {
							DestroyImmediate(cElement, false);
						}
						if (jblockHash.Contains(palPrefab)) {
							var jblocks = blockTF.GetComponents<JBlock>();
							foreach (var jblock in jblocks) {
								jblock.OnBlockLoaded(null, block);
							}
						}
					}
				}
			} catch (System.Exception ex) {
				DestroyImmediate(rootTF.gameObject, false);
				DestroyImmediate(dontCombineRoot.gameObject, false);
				rootTF = null;
				Debug.LogWarning(ex);
			}
		}


		private void Combine_MoveOutDontCombineBlocks (Transform rootTF, Transform dontCombineRoot) {
			int layerCount = rootTF.childCount;
			for (int i = 0; i < layerCount; i++) {
				var layerTF = rootTF.GetChild(i);
				int blockCount = layerTF.childCount;
				for (int j = 0; j < blockCount; j++) {
					var blockTF = layerTF.GetChild(j);
					if (blockTF.name == DONT_COMBINE_NAME) {
						blockTF.SetParent(dontCombineRoot, true);
						var fakeBlock = new GameObject(DONT_COMBINE_NAME).transform;
						fakeBlock.SetParent(layerTF);
						fakeBlock.SetSiblingIndex(j);
						fakeBlock.localPosition = blockTF.localPosition;
						fakeBlock.localRotation = blockTF.localRotation;
						fakeBlock.localScale = blockTF.localScale;
					}
				}
			}
		}


		private void Combine_IgnoreOverlap (Transform rootTF, ConfigItem[] configItems) {

			if (!IgnoreOverlappingTriangle) { return; }

			var map = Map;
			int layerCount = map.LayerCount;

			// Init Pos Cache
			var posHash = new HashSet<Vector3Int>();
			for (int i = 0; i < layerCount && i < configItems.Length; i++) {
				var configItem = configItems[i];
				if (!configItem.CombineMesh) { continue; }
				var layer = map[i];
				var layerTF = rootTF.GetChild(i);
				int blockCount = layer.BlockCount;
				for (int j = 0; j < blockCount; j++) {
					var blockTF = layerTF.GetChild(j);
					if (blockTF.name != OVERLAP_NEARBY_NAME) { continue; }
					var block = layer[j];
					var pos = block.Position;
					if (!posHash.Contains(pos)) {
						posHash.Add(pos);
					}
				}
			}

			// Init Overlap Cache
			var overlapMap = new Dictionary<(int layerIndex, int blockIndex), BlockOverlap>();
			for (int i = 0; i < layerCount && i < configItems.Length; i++) {
				var configItem = configItems[i];
				if (!configItem.CombineMesh) { continue; }
				var layer = map[i];
				int blockCount = layer.BlockCount;
				for (int j = 0; j < blockCount; j++) {
					var block = layer[j];
					var pos = block.Position;
					var overlap = BlockOverlap.None;
					if (posHash.Contains(new Vector3Int(pos.x - 1, pos.y, pos.z))) {
						overlap |= BlockOverlap.L;
					}
					if (posHash.Contains(new Vector3Int(pos.x + 1, pos.y, pos.z))) {
						overlap |= BlockOverlap.R;
					}
					if (posHash.Contains(new Vector3Int(pos.x, pos.y - 1, pos.z))) {
						overlap |= BlockOverlap.D;
					}
					if (posHash.Contains(new Vector3Int(pos.x, pos.y + 1, pos.z))) {
						overlap |= BlockOverlap.U;
					}
					if (posHash.Contains(new Vector3Int(pos.x, pos.y, pos.z - 1))) {
						overlap |= BlockOverlap.B;
					}
					if (posHash.Contains(new Vector3Int(pos.x, pos.y, pos.z + 1))) {
						overlap |= BlockOverlap.F;
					}
					overlapMap.Add((i, j), overlap);
				}
			}

			// Comvert To Overlap Fixed Map Transform
			int layerTFCount = rootTF.childCount;
			var deleteList = new List<GameObject>();
			for (int i = 0; i < layerTFCount && i < configItems.Length; i++) {
				var configItem = configItems[i];
				if (!configItem.CombineMesh) { continue; }
				var layerTF = rootTF.GetChild(i);
				int blockTFCount = layerTF.childCount;
				for (int j = 0; j < blockTFCount; j++) {
					if (!overlapMap.ContainsKey((i, j))) { continue; }
					var overlap = overlapMap[(i, j)];
					var blockTF = layerTF.GetChild(j);
					if (overlap == BlockOverlap.All) {
						// Delete
						deleteList.Add(blockTF.gameObject);
					} else if (overlap != BlockOverlap.None) {
						// Fix
						FixOverlapForBlock(blockTF, overlap);
					}
				}
			}

			// Delete
			foreach (var g in deleteList) {
				DestroyImmediate(g, false);
			}

		}


		private void Combine_FindMeshFilters (Transform rootTF, ConfigItem[] configItems, out Dictionary<int, List<(MeshFilter, MeshRenderer)>> meshFilterMap) {
			int layerCount = rootTF.childCount;
			meshFilterMap = new Dictionary<int, List<(MeshFilter, MeshRenderer)>>();
			var combineMode = (CombineMode)CombineModeIndex.Value;
			for (int i = 0; i < layerCount; i++) {
				if (!configItems[i].CombineMesh) { continue; }
				int index = combineMode == CombineMode.OneMeshForWholeMap ? -1 : i;
				var list = meshFilterMap.ContainsKey(index) ? meshFilterMap[index] : new List<(MeshFilter, MeshRenderer)>();
				GetComponentsIn(rootTF.GetChild(i), list);
				if (!meshFilterMap.ContainsKey(index)) {
					meshFilterMap.Add(index, list);
				}
			}
		}


		private void Combine_PackTexture (string rootName, Dictionary<int, List<(MeshFilter mf, MeshRenderer mr)>> meshFilterMap, out Texture2D texture, out Dictionary<Texture2D, (Rect from, Rect to)> uvRemap) {

			uvRemap = new Dictionary<Texture2D, (Rect from, Rect to)>();

			// Get Trim Map
			var trimRectMap = new Dictionary<MeshFilter, Rect>();
			var trimRectMap_Texture = new Dictionary<Texture2D, Rect>();
			foreach (var pair in meshFilterMap) {
				var list = pair.Value;
				foreach (var (mf, mr) in list) {
					var tex = GetTextureFromMaterial(mr.sharedMaterial);
					if (tex == null || mf.sharedMesh == null) { continue; }
					var trimRect = trimRectMap_Texture.ContainsKey(tex) ? trimRectMap_Texture[tex] : new Rect(1, 1, -1, -1);
					Rect currentRect;
					if (trimRectMap.ContainsKey(mf)) {
						currentRect = trimRectMap[mf];
					} else {
						float xMin = float.MaxValue;
						float xMax = float.MinValue;
						float yMin = float.MaxValue;
						float yMax = float.MinValue;
						var uvs = mf.sharedMesh.uv;
						if (uvs.Length == 0) { continue; }
						foreach (var uv in uvs) {
							xMin = Mathf.Min(xMin, Mathf.Clamp01(uv.x));
							xMax = Mathf.Max(xMax, Mathf.Clamp01(uv.x));
							yMin = Mathf.Min(yMin, Mathf.Clamp01(uv.y));
							yMax = Mathf.Max(yMax, Mathf.Clamp01(uv.y));
						}
						currentRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
						trimRectMap.Add(mf, currentRect);
					}
					trimRect = Rect.MinMaxRect(
						Mathf.Min(trimRect.xMin, currentRect.xMin),
						Mathf.Min(trimRect.yMin, currentRect.yMin),
						Mathf.Max(trimRect.xMax, currentRect.xMax),
						Mathf.Max(trimRect.yMax, currentRect.yMax)
					);
					if (trimRectMap_Texture.ContainsKey(tex)) {
						trimRectMap_Texture[tex] = trimRect;
					} else {
						trimRectMap_Texture.Add(tex, trimRect);
					}
				}
			}

			// Get Textures
			var sourceTextures = new List<Texture2D>();
			var textures = new List<Texture2D>();
			var trimRects = new List<Rect>();
			var textureHash = new HashSet<Texture2D>();
			int pointFilterCount = 0;
			foreach (var pair in meshFilterMap) {
				var list = pair.Value;
				foreach (var (_, mr) in list) {
					var tex = GetTextureFromMaterial(mr.sharedMaterial);
					if (tex == null || textureHash.Contains(tex)) { continue; }
					var trimRect = trimRectMap_Texture.ContainsKey(tex) ? trimRectMap_Texture[tex] : new Rect(0, 0, 1, 1);
					textureHash.Add(tex);
					sourceTextures.Add(tex);
					textures.Add(GetTrimedTexture(tex, trimRect));
					trimRects.Add(trimRect);
					pointFilterCount += tex.filterMode == FilterMode.Point ? 1 : -1;
				}
			}

			// Pack Texture
			texture = new Texture2D(1, 1) {
				name = rootName,
				alphaIsTransparency = false,
				filterMode = pointFilterCount >= 0 ? FilterMode.Point : FilterMode.Bilinear,
			};
			var uvRects = texture.PackTextures(textures.ToArray(), 1);
			if (uvRects == null) {
				Debug.LogWarning("[Jujube Combiner] Fail to pack textures.");
				return;
			}

			// Fill UvRemap
			for (int i = 0; i < textures.Count; i++) {
				uvRemap.Add(sourceTextures[i], (trimRects[i], uvRects[i]));
			}

		}


		private void Combine_CombineMeshs (Transform rootTF, Dictionary<int, List<(MeshFilter mf, MeshRenderer mr)>> meshFilterMap, Texture2D texture, Dictionary<Texture2D, (Rect from, Rect to)> uvRemap, out Material material, out List<Mesh> meshs) {

			var shader = PrefabShader;
			material = new Material(shader) { name = rootTF.name, };
			meshs = new List<Mesh>();
			material.SetTexture(Shader.PropertyToID(ShaderTextureKeyword), texture);
			if (uvRemap == null) {
				uvRemap = new Dictionary<Texture2D, (Rect from, Rect to)>();
			}
			var DEFAULT_UV = Vector2.one * 0.5f;

			// Combine Meshs
			foreach (var pair in meshFilterMap) {
				int fIndex = pair.Key;
				var list = pair.Value;
				if (list.Count == 0) { continue; }

				// Fill Data
				var vs = new List<Vector3>();
				var tris = new List<int>();
				var uvs = new List<Vector2>();
				foreach (var (mf, mr) in list) {

					var tex = GetTextureFromMaterial(mr.sharedMaterial);
					//if (tex == null || !uvRemap.ContainsKey(tex)) { continue; }
					var mesh = mf.sharedMesh;
					if (mesh == null) { continue; }

					var _vs = mesh.vertices;
					var _tris = mesh.triangles;
					var _uvs = mesh.uv;
					int _vLen = _vs.Length;
					int _triLen = _tris.Length;
					int _uvLen = _uvs.Length;
					int vertOffset = vs.Count;
					if (_vLen == 0 || _triLen == 0 || _uvLen == 0 || _vLen != _uvLen) { continue; }

					// Vert
					var matrix = mf.transform.localToWorldMatrix;
					for (int i = 0; i < _vLen; i++) {
						vs.Add(matrix.MultiplyPoint3x4(_vs[i]));
					}

					// Tri
					for (int i = 0; i < _triLen; i++) {
						tris.Add(_tris[i] + vertOffset);
					}

					// UV
					if (tex != null && uvRemap.ContainsKey(tex)) {
						var (fromRect, toRect) = uvRemap[tex];
						Vector2 uvFromMin = fromRect.min;
						Vector2 uvFromMax = fromRect.max;
						Vector2 uvToMin = toRect.min;
						Vector2 uvToMax = toRect.max;
						for (int i = 0; i < _uvLen; i++) {
							Vector2 uv = _uvs[i];
							uv.x = EditorUtil.RemapUnclamped(uvFromMin.x, uvFromMax.x, uvToMin.x, uvToMax.x, uv.x);
							uv.y = EditorUtil.RemapUnclamped(uvFromMin.y, uvFromMax.y, uvToMin.y, uvToMax.y, uv.y);
							uvs.Add(uv);
						}
					} else {
						for (int i = 0; i < _uvLen; i++) {
							uvs.Add(DEFAULT_UV);
						}
					}

				}

				// Create Mesh
				var resultMesh = new Mesh() {
					name = $"{rootTF.name}_{(fIndex < 0 ? "combined" : rootTF.GetChild(fIndex).name)}",
					indexFormat = UnityEngine.Rendering.IndexFormat.UInt32,
				};
				resultMesh.SetVertices(vs);
				resultMesh.SetTriangles(tris, 0);
				resultMesh.SetUVs(0, uvs);
				resultMesh.RecalculateBounds();
				resultMesh.RecalculateNormals();
				resultMesh.RecalculateTangents();
				resultMesh.UploadMeshData(false);
				meshs.Add(resultMesh);

				// Setup Target Transform
				var targetTF = fIndex < 0 ? rootTF : rootTF.GetChild(fIndex);
				var resultMR = targetTF.gameObject.AddComponent<MeshRenderer>();
				var resultMF = targetTF.gameObject.AddComponent<MeshFilter>();
				resultMR.sharedMaterial = material;
				resultMF.sharedMesh = resultMesh;

			}



		}


		private void Combine_ClearRootTF (Transform rootTF, Transform dontCombineRoot, ConfigItem[] configItems) {
			var combineMode = (CombineMode)CombineModeIndex.Value;
			var deleteList = new List<GameObject>();

			// Layers
			if (combineMode == CombineMode.OneMeshForWholeMap) {
				int layerCount = rootTF.childCount;
				for (int i = 0; i < layerCount; i++) {
					var layerTF = rootTF.GetChild(i);
					if (configItems[i].CombineMesh) {
						deleteList.Add(layerTF.gameObject);
					}
				}
			} else {
				int layerCount = rootTF.childCount;
				for (int i = 0; i < layerCount; i++) {
					var layerTF = rootTF.GetChild(i);
					if (!configItems[i].CombineMesh) { continue; }
					int blockCount = layerTF.childCount;
					for (int j = 0; j < blockCount; j++) {
						deleteList.Add(layerTF.GetChild(j).gameObject);
					}
				}
			}

			// Move Dont Combine Out
			if (dontCombineRoot.childCount == 0) {
				DestroyImmediate(dontCombineRoot.gameObject, false);
			} else {
				dontCombineRoot.SetParent(rootTF, true);
			}

			// Delete
			foreach (var g in deleteList) {
				DestroyImmediate(g, false);
			}
		}


		private void Combine_SavePrefab (string path, Transform rootTF, Material material, Texture2D texture, List<Mesh> meshs) {
			int mainTexID = Shader.PropertyToID(ShaderTextureKeyword);
			material.SetTexture(mainTexID, texture);
			if (EditorUtil.FileExists(path)) {
				AssetDatabase.DeleteAsset(path);
			}
			var tempObj = new GameObject();
			var prefab = PrefabUtility.SaveAsPrefabAsset(tempObj, path);
			DestroyImmediate(tempObj, false);
			if (prefab != null) {
				AssetDatabase.AddObjectToAsset(material, path);
				AssetDatabase.AddObjectToAsset(texture, path);
				for (int i = 0; i < meshs.Count; i++) {
					AssetDatabase.AddObjectToAsset(meshs[i], path);
				}
				prefab = PrefabUtility.SaveAsPrefabAsset(rootTF.gameObject, path);
				EditorGUIUtility.PingObject(prefab);
			}
		}


		// Load-Save
		private void LoadConfig () {
			ConfigMap.Clear();
			string configData = string.Empty;
			if (EditorUtil.FileExists(ConfigPath)) {
				var config = AssetDatabase.LoadAssetAtPath<TextAsset>(ConfigPath);
				if (config != null) {
					configData = config.text;
				}
			}
			if (!string.IsNullOrEmpty(configData)) {
				string[] cDatas = configData.Replace("\r", string.Empty).Split('\n');
				int len = cDatas.Length;
				for (int i = 0; i < len; i++) {
					string cData = cDatas[i];
					if (string.IsNullOrEmpty(cData)) { continue; }
					int keyLen = cData.IndexOf(':');
					if (keyLen < 0 || keyLen == cData.Length - 1) { continue; }
					string key = cData.Substring(0, keyLen);
					if (ConfigMap.ContainsKey(key)) { continue; }
					string values = cData.Substring(keyLen + 1, cData.Length - keyLen - 1);
					string[] valueDatas = values.Split(',');
					long changedTime = 0;
					if (valueDatas.Length > 1) {
						long.TryParse(valueDatas[1], out changedTime);
					}
					var value = new ConfigItem() {
						CombineMesh = valueDatas.Length == 0 || valueDatas[0] == "1",
						ChangedTime = changedTime > 0 ? changedTime : EditorUtil.GetLongTime(),
					};
					ConfigMap.Add(key, value);
				}
			} else {
				EditorUtil.TextToFile(string.Empty, ConfigPath);
			}
		}


		private void SaveConfig () {
			// Count Check
			if (ConfigMap.Count > MAX_CONFIG_DATA_COUNT) {
				var list = new List<(string key, ConfigItem value)>();
				foreach (var pair in ConfigMap) {
					list.Add((pair.Key, pair.Value));
				}
				list.Sort((a, b) => b.value.ChangedTime.CompareTo(a.value.ChangedTime));
				list.RemoveRange(MAX_CONFIG_DATA_COUNT, list.Count - MAX_CONFIG_DATA_COUNT);
				ConfigMap.Clear();
				foreach (var (key, value) in list) {
					if (!ConfigMap.ContainsKey(key)) {
						ConfigMap.Add(key, value);
					}
				}
			}
			// Save
			var builder = new StringBuilder();
			foreach (var pair in ConfigMap) {
				if (string.IsNullOrEmpty(pair.Key) || pair.Value == null) { continue; }
				// Key
				builder.Append(pair.Key);
				builder.Append(':');
				// CombineMesh
				builder.Append(pair.Value.CombineMesh ? '1' : '0');
				builder.Append(',');
				// Changed Time
				builder.Append(pair.Value.ChangedTime.ToString());
				builder.Append(',');
				builder.Append('\n');
			}
			EditorUtil.TextToFile(builder.ToString(), ConfigPath);
		}


		// Field
		private void ShowShaderPropertyDropdown () {
			var shader = PrefabShader;
			if (shader != null) {
				int propertyCount = shader.GetPropertyCount();
				string propertyName = TextureKeyword.Value;
				var menu = new GenericMenu() { allowDuplicateNames = false, };
				for (int i = 0; i < propertyCount; i++) {
					if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) { continue; }
					string proName = shader.GetPropertyName(i);
					menu.AddItem(
						new GUIContent(proName),
						proName == propertyName,
						() => TextureKeyword.Value = proName
					);
				}
				menu.ShowAsContext();
			}
		}


		// Util
		private Vector3 GetPrefabLocalScale (Transform prefab) {
			if (prefab == null) { return Vector3.one; }
			switch (PrefabScale) {
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


		private Texture2D GetTextureFromMaterial (Material mat) {
			if (mat == null) { return null; }
			Texture2D result = mat.mainTexture as Texture2D;
			if (result == null) {
				MaterialPropertyIdCache.Clear();
				mat.GetTexturePropertyNameIDs(MaterialPropertyIdCache);
				foreach (var id in MaterialPropertyIdCache) {
					result = mat.GetTexture(id) as Texture2D;
					if (result != null) { break; }
				}
			}
			return result;
		}


		private Texture2D GetTrimedTexture (Texture2D texture, Rect trimRect) {
			int width = texture.width;
			int height = texture.height;
			int newWidth = (int)(width * trimRect.width);
			int newHeight = (int)(height * trimRect.height);
			var result = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
			result.SetPixels(texture.GetPixels(
				(int)(trimRect.x * width),
				(int)(trimRect.y * height),
				newWidth,
				newHeight
			));
			result.Apply();
			return result;
		}


		private void GetComponentsIn<T, U> (Transform target, List<(T, U)> result) where T : Component where U : Component {
			var tempMFResult = new List<T>();
			var tempMfMrResult = new List<(T, U)>();
			int count = target.childCount;
			for (int j = 0; j < count; j++) {
				var blockTF = target.GetChild(j);
				tempMFResult.Clear();
				blockTF.GetComponentsInChildren(tempMFResult);
				var tempMF = blockTF.GetComponent<T>();
				if (tempMF != null) {
					tempMFResult.Add(tempMF);
				}
				tempMfMrResult.Clear();
				foreach (var mf in tempMFResult) {
					var tempMR = mf.GetComponent<U>();
					if (tempMR == null) { continue; }
					tempMfMrResult.Add((mf, tempMR));
				}
				result.AddRange(tempMfMrResult);
			}
		}


		private void FixOverlapForBlock (Transform blockTF, BlockOverlap overlap) {

			// Get Meshs
			var tempMfMrList = new List<(MeshFilter, MeshRenderer)>();
			GetComponentsIn(blockTF, tempMfMrList);
			Vector3 centerPos = blockTF.position - PivotOffset;
			foreach (var (mf, _) in tempMfMrList) {
				var mesh = mf.sharedMesh;
				if (mesh == null) { continue; }
				var vs = mesh.vertices;
				var tris = mesh.triangles;
				var uvs = mesh.uv;
				int vLen = vs.Length;
				int tLen = tris.Length;
				int uvLen = uvs.Length;
				if (vLen == 0 || tLen == 0 || uvLen == 0 || vLen != uvLen) { continue; }
				var resultVs = new List<Vector3>();
				var resultUVs = new List<Vector2>();
				var resultTris = new List<int>();
				var matrix = mf.transform.localToWorldMatrix;
				// Fill Result List
				for (int i = 0; i < tLen; i += 3) {
					int t0 = tris[i + 0];
					int t1 = tris[i + 1];
					int t2 = tris[i + 2];
					Vector3 v0 = vs[t0];
					Vector3 v1 = vs[t1];
					Vector3 v2 = vs[t2];
					Vector2 uv0 = uvs[t0];
					Vector2 uv1 = uvs[t1];
					Vector2 uv2 = uvs[t2];
					// Check Direction
					if (overlap.HasFlag(GetOverlap((matrix.MultiplyPoint3x4(v0) + matrix.MultiplyPoint3x4(v1) + matrix.MultiplyPoint3x4(v2)) / 3f - centerPos))) {
						continue;
					}
					// Fill
					resultVs.Add(v0);
					resultVs.Add(v1);
					resultVs.Add(v2);
					resultUVs.Add(uv0);
					resultUVs.Add(uv1);
					resultUVs.Add(uv2);
					int triIndex = resultTris.Count;
					resultTris.Add(triIndex + 0);
					resultTris.Add(triIndex + 1);
					resultTris.Add(triIndex + 2);
				}
				// Make Result Mesh
				var resultMesh = new Mesh();
				resultMesh.SetVertices(resultVs);
				resultMesh.SetUVs(0, resultUVs);
				resultMesh.SetTriangles(resultTris, 0);
				resultMesh.RecalculateBounds();
				resultMesh.RecalculateNormals();
				resultMesh.RecalculateTangents();
				resultMesh.UploadMeshData(false);
				mf.sharedMesh = resultMesh;
			}
			// Func
			BlockOverlap GetOverlap (Vector3 dir) {
				float _x = Mathf.Abs(dir.x);
				float _y = Mathf.Abs(dir.y);
				float _z = Mathf.Abs(dir.z);
				if (_x > _y && _x > _z) {
					return dir.x < 0f ? BlockOverlap.L : BlockOverlap.R;
				} else if (_y > _x && _y > _z) {
					return dir.y < 0f ? BlockOverlap.D : BlockOverlap.U;
				} else {
					return dir.z < 0f ? BlockOverlap.B : BlockOverlap.F;
				}
			}
		}


		#endregion




	}
}