namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;


	[CustomEditor(typeof(JujubeGenerator))]
	public class JujubeGenerator_Inspector : MoenenInspector {


		// Data
		private SerializedProperty[] Props_A = null;
		private SerializedProperty[] Props_B = null;
		private int PrevSizeX = 0;
		private int PrevSizeY = 0;
		private static EditorSavingBool GroundPanelOpen = new EditorSavingBool("JujubeGenerator_Inspector.GroundPanelOpen", false);
		private static EditorSavingBool WaterPanelOpen = new EditorSavingBool("JujubeGenerator_Inspector.WaterPanelOpen", false);
		private static EditorSavingBool ItemPanelOpen = new EditorSavingBool("JujubeGenerator_Inspector.ItemPanelOpen", false);
		private readonly Dictionary<GameObject, Texture2D> PreviewTextureMap = new Dictionary<GameObject, Texture2D>();


		// MSG
		private void OnEnable () {
			if (serializedObject != null) {
				Props_A = new SerializedProperty[]{
					serializedObject.FindProperty("m_Palette"),
				};
				Props_B = new SerializedProperty[]{
					serializedObject.FindProperty("m_CellSize"),
					serializedObject.FindProperty("m_PrefabScale"),
					serializedObject.FindProperty("m_PrefabPivot"),
					serializedObject.FindProperty("m_LoadAsShell"),
				};
			}
			var jTarget = target as JujubeGenerator;
			if (jTarget != null) {
				PrevSizeX = jTarget.SizeX;
				PrevSizeY = jTarget.SizeY;
				if (jTarget.transform.childCount == 0 && jTarget.Palette != null && jTarget.Palette.Count != 0) {
					jTarget.GenerateAndLoad(LogProgress);
				}
			}
		}


		public override void OnInspectorGUI () {
			GUI_Properties();
			var jTarget = target as JujubeGenerator;
			if (jTarget.Palette != null && jTarget.Palette.Count > 0) {
				Space(2);
				GUI_Control();
				Space(10);
				GUI_StepGround(jTarget);
				GUI_StepWater(jTarget);
				GUI_StepItem(jTarget);
				Space(2);
				if (GUI.changed) {
					EditorUtility.SetDirty(jTarget);
				}
			}
			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl(string.Empty);
			}
		}


		private void GUI_Properties () {

			serializedObject.Update();

			// Property A
			Space(4);
			foreach (var prop in Props_A) {
				EditorGUILayout.PropertyField(prop);
			}

			var jTarget = target as JujubeGenerator;
			if (jTarget.Palette == null) {
				// No Palette 
				Space(2);
				if (GUI.Button(GUIRect(0, 24), "Create Palette Asset")) {
					var pal = CreateAsset<JujubePalette>(
						"New Jujube Palette",
						"[Jujube Map Editor] Fail to create palette asset."
					);
					if (pal != null) {
						string path = EditorUtil.CombinePaths(EditorUtil.GetRootPath(), "Core", "Failback Cube.prefab");
						if (EditorUtil.FileExists(path)) {
							var cubePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
							if (cubePrefab != null) {
								pal.Failback = cubePrefab;
								pal.AddItem(cubePrefab);
							}
						}
						jTarget.SetPalette(pal);
					}
					EditorUtility.SetDirty(jTarget);
				}
			} else if (jTarget.Palette.Count == 0) {
				// Palette Empty
				Space(2);
				EditorGUI.HelpBox(GUIRect(0, 36), "Palette is empty, add prefabs into this palette before generate map.", MessageType.Warning);
			} else {
				// Property B
				foreach (var prop in Props_B) {
					EditorGUILayout.PropertyField(prop);
				}
				Space(8);
				EditorGUI.LabelField(GUIRect(0, 18), "Generation");
				// Size
				jTarget.SizeX = EditorGUILayout.DelayedIntField("Size X", jTarget.SizeX);
				jTarget.SizeY = EditorGUILayout.DelayedIntField("Size Y", jTarget.SizeY);
				// Seed
				LayoutH(() => {
					EditorGUI.LabelField(GUIRect(0, 22), "Seed", BytesToString(jTarget.Seed, 12) + "...");
					// Dice
					if (GUI.Button(GUIRect(28, 22), new GUIContent(GetJujubeImage($"Dice Icon_{(EditorGUIUtility.isProSkin ? "pro" : "per")}.png")))) {
						jTarget.CalculateSeed();
						try {
							jTarget.GenerateAndLoad(LogProgress);
						} catch (System.Exception ex) { Debug.LogError(ex); }
						EditorUtility.SetDirty(jTarget);
					}
				});
				Space(2);
			}

			serializedObject.ApplyModifiedProperties();

			// Resize Check
			if (PrevSizeX != jTarget.SizeX || PrevSizeY != jTarget.SizeY) {
				PrevSizeX = jTarget.SizeX;
				PrevSizeY = jTarget.SizeY;
				jTarget.CalculateSeed();
				EditorUtility.SetDirty(jTarget);
			}
		}


		private void GUI_Control () {
			const int ITEM_HEIGHT = 24;
			var jTarget = target as JujubeGenerator;

			// Buttons
			LayoutH(() => {

				// Generate
				if (GUI.Button(GUIRect(0, ITEM_HEIGHT), new GUIContent("Generate"))) {
					try {
						jTarget.GenerateAndLoad(LogProgress);
					} catch (System.Exception ex) { Debug.LogError(ex); }
					EditorUtil.ClearProgressBar();
				}

			});


			// Export
			LayoutH(() => {

				// Export Map
				if (GUI.Button(GUIRect(0, ITEM_HEIGHT), new GUIContent(" Export Map", GetJujubeImage("Map Icon.png")))) {
					JujubeMap map = null;
					try {
						map = jTarget.Generate(LogProgress);
					} catch (System.Exception ex) { Debug.LogError(ex); }
					EditorUtil.ClearProgressBar();
					if (map != null) {
						EditorApplication.delayCall += () => {
							string path = EditorUtility.SaveFilePanelInProject("Export Map", "New Jujube Map", "asset", "Export generated map");
							if (!string.IsNullOrEmpty(path)) {
								path = EditorUtil.FixedRelativePath(path);
								AssetDatabase.CreateAsset(map, path);
								EditorGUIUtility.PingObject(map);
							}
						};
					}
				}

				// Export Mesh
				if (GUI.Button(GUIRect(0, ITEM_HEIGHT), new GUIContent(" Export Mesh", GetJujubeImage("Mesh Icon.png")))) {
					JujubeMap map = null;
					try {
						map = jTarget.Generate(LogProgress);
					} catch (System.Exception ex) { Debug.LogError(ex); }
					if (map != null) {
						var window = JujubeCombinerWindow.OpenCombinerWindow();
						window.Map = map;
						window.Palette = jTarget.Palette;
						window.titleContent = new GUIContent("Export Mesh");
						window.PrefabPivot = jTarget.PrefabPivot;
						window.PrefabScale = jTarget.PrefabScale;
						window.CellSize = jTarget.CellSize;
					}
				}

			});

		}


		private void GUI_StepGround (JujubeGenerator jTarget) {
			if (jTarget.UseGround) {
				// Ground
				bool useGround = jTarget.UseGround;
				bool open = GroundPanelOpen.Value;
				LayoutF_Double(() => {
					PrefabsField(jTarget.Ground_Prefabs);
					jTarget.Ground_Iteration = EditorGUILayout.IntField("Iteration", jTarget.Ground_Iteration);
					jTarget.Ground_IterationRadius = EditorGUILayout.FloatField("Iteration Radius", jTarget.Ground_IterationRadius);
					jTarget.Ground_MinHeight = EditorGUILayout.IntField("Min Height", jTarget.Ground_MinHeight);
					jTarget.Ground_MaxHeight = EditorGUILayout.IntField("Max Height", jTarget.Ground_MaxHeight);
					jTarget.Ground_Style = (JujubeGenerator.GroundStyle)EditorGUILayout.EnumPopup("Style", jTarget.Ground_Style);
					if (jTarget.Ground_Style != JujubeGenerator.GroundStyle.Mainland) {
						jTarget.Ground_Edge = EditorGUILayout.IntField("Edge", jTarget.Ground_Edge);
					}
					jTarget.Ground_RotatePrefab = EditorGUILayout.Toggle("Rotate Prefab", jTarget.Ground_RotatePrefab);
				}, "Ground", ref open, ref useGround, true);
				jTarget.UseGround = useGround;
				GroundPanelOpen.Value = open;
			} else {
				// No Ground
				LayoutH(() => {
					Space(6);
					jTarget.UseGround = EditorGUI.ToggleLeft(GUIRect(0, 18), " Use Ground", jTarget.UseGround);
				});
			}
			Space(2);
		}


		private void GUI_StepWater (JujubeGenerator jTarget) {
			if (jTarget.UseWater) {
				// Water
				bool useWater = jTarget.UseWater;
				bool open = WaterPanelOpen.Value;
				LayoutF_Double(() => {
					PrefabsField(jTarget.Water_Prefabs);
					jTarget.Water_Height = EditorGUILayout.IntField("Height", jTarget.Water_Height);
					jTarget.Water_RotatePrefab = EditorGUILayout.Toggle("Rotate Prefab", jTarget.Water_RotatePrefab);

				}, "Water", ref open, ref useWater, true);
				jTarget.UseWater = useWater;
				WaterPanelOpen.Value = open;
			} else {
				// No Water
				LayoutH(() => {
					Space(6);
					jTarget.UseWater = EditorGUI.ToggleLeft(GUIRect(0, 18), " Use Water", jTarget.UseWater);
				});
			}
			Space(2);
		}


		private void GUI_StepItem (JujubeGenerator jTarget) {
			if (jTarget.UseItem) {
				// Item
				bool useItem = jTarget.UseItem;
				bool open = ItemPanelOpen.Value;
				int showMenu = -1;
				LayoutF_Double(() => {
					Space(2);
					int count = jTarget.Items.Count;
					for (int i = 0; i < count; i++) {
						// Item
						var item = jTarget.Items[i];
						bool itemOpen = item.Editor_OpenedInInspector;
						LayoutH(() => {
							GUI.Label(GUIRect(10, 28), i.ToString(), EditorStyles.centeredGreyMiniLabel);
							// Content
							LayoutF_Button(() => {

								// Property
								PrefabsField(item.Prefabs, string.Empty);
								item.Location = (JujubeGenerator.LocationType)EditorGUILayout.EnumFlagsField("Location", item.Location);
								GUI.Label(GUIRect(0, 18), "Probability");
								LayoutH(() => {
									Space(24);
									int newP = (int)GUI.HorizontalSlider(GUIRect(0, 18), item.Probability, 0, 100);
									if (newP != item.Probability) {
										item.Probability = Mathf.RoundToInt(newP / 10f) * 10;
									}
									Space(4);
									item.Probability = EditorGUI.IntField(GUIRect(32, 18), item.Probability);
									GUI.Label(GUIRect(12, 18), "%");
								}, false, GUIStyle.none);
								item.MinHeight = EditorGUILayout.IntField("Min Height", item.MinHeight);
								item.MaxHeight = EditorGUILayout.IntField("Max Height", item.MaxHeight);
								item.AllowOverlap = EditorGUILayout.Toggle("Allow Overlap", item.AllowOverlap);
								item.RotatePrefab = EditorGUILayout.Toggle("Rotate Prefab", item.RotatePrefab);
								Space(2);
							}, (labelRect) => {
								if (itemOpen) {
									GUI.Label(labelRect, "Prefab");
								} else if (item.Prefabs != null && jTarget.Palette != null) {
									var rect = labelRect;
									rect.width = rect.height;
									for (int j = 0; j < item.Prefabs.Count && j <= 6; j++) {
										int prefabIndex = item.Prefabs[j];
										var prefab = prefabIndex >= 0 && prefabIndex < jTarget.Palette.Count ? jTarget.Palette[prefabIndex] : null;
										var preview = prefab != null ? GetPreview(prefab) : null;
										GUI.Box(rect, GUIContent.none, GUI.skin.button);
										if (preview != null) {
											GUI.DrawTexture(ShrinkRect(rect, 2), preview);
										}
										rect.x += rect.width;
									}
									if (item.Prefabs.Count > 6) {
										GUI.Label(labelRect, "...");
									}
								}
							}, () => showMenu = i, ref itemOpen, false, RichHelpBox);
							item.Editor_OpenedInInspector = itemOpen;
						});
						Space(2);
					}
					Space(6);
					// Add
					LayoutH(() => {
						GUIRect(0, 18);
						if (GUI.Button(GUIRect(58, 18), "+ Item")) {
							jTarget.Items.Add(new JujubeGenerator.ItemData());
						}
					});
					Space(2);
				}, "Item", ref open, ref useItem, true);
				jTarget.UseItem = useItem;
				ItemPanelOpen.Value = open;
				// Menu
				if (showMenu >= 0) {
					ShowItemPopMenu(jTarget, showMenu);
				}
			} else {
				// No Item
				LayoutH(() => {
					Space(6);
					jTarget.UseItem = EditorGUI.ToggleLeft(GUIRect(0, 18), " Use Item", jTarget.UseItem);
				});
			}
			Space(2);
		}


		// LGC
		private string BytesToString (byte[] bytes, int len) {
			if (bytes == null || bytes.Length == 0) { return string.Empty; }
			string result = string.Empty;
			for (int i = 0; i < len && i < bytes.Length; i++) {
				result += (char)(bytes[i] + '0');
			}
			return result;
		}


		private void LogProgress (float progress) {
			if (progress >= 1f) {
				EditorUtil.ClearProgressBar();
			} else {
				EditorUtil.ClearProgressBar();
				EditorUtil.ProgressBar("Generating", "Generating Map Data...", progress);
			}
		}


		private void PrefabsField (List<int> prefabs, string label = "Prefabs") {
			var pal = (target as JujubeGenerator).Palette;
			const int ITEM_SIZE = 32;
			int len = prefabs.Count;
			int prefabCountX = Mathf.FloorToInt((EditorGUIUtility.currentViewWidth - 36f) / ITEM_SIZE);
			int prefabCountY = Mathf.CeilToInt((float)(len + 2) / prefabCountX);
			int index = 0;
			int showMenu = -1;
			bool addNew = false;
			bool removeLast = false;
			Space(2);
			if (!string.IsNullOrEmpty(label)) {
				GUI.Label(GUIRect(0, 18), label);
			}
			for (int y = 0; y < prefabCountY && index < len + 2; y++) {
				LayoutH(() => {
					Space(8);
					for (int x = 0; x < prefabCountX; x++, index++) {
						if (index < len) {
							// Prefab
							int prefabIndex = prefabs[index];
							var prefab = prefabIndex >= 0 && prefabIndex < pal.Count ? pal[prefabIndex] : null;
							var preview = prefab != null ? GetPreview(prefab) : null;
							var rect = GUIRect(0, ITEM_SIZE);
							rect.height = rect.width;
							rect = ShrinkRect(rect, 1);
							if (GUI.Button(rect, GUIContent.none)) {
								showMenu = index;
							}
							// Preview
							if (preview != null) {
								GUI.Box(rect, GUIContent.none, GUI.skin.button);
								GUI.DrawTexture(ShrinkRect(rect, 2.5f), preview);
							} else {
								GUI.Box(rect, GUIContent.none, GUI.skin.button);
							}
						} else if (index == len) {
							// Remove
							var rect = GUIRect(0, ITEM_SIZE);
							rect.height = rect.width;
							rect = ShrinkRect(rect, 1);
							if (GUI.Button(rect, "-") && len > 0) {
								removeLast = true;
							}
						} else if (index == len + 1) {
							// Add
							var rect = GUIRect(0, ITEM_SIZE);
							rect.height = rect.width;
							rect = ShrinkRect(rect, 1);
							if (GUI.Button(rect, "+")) {
								addNew = true;
							}
						} else {
							// Empty
							GUIRect(0, ITEM_SIZE);
						}
					}
				});
			}

			// New
			if (addNew) {
				prefabs.Add(0);
			}

			// Remove
			if (removeLast) {
				prefabs.RemoveAt(prefabs.Count - 1);
			}

			// Menu
			if (showMenu >= 0) {
				ShowPrefabPopMenu(prefabs, pal, showMenu);
			}

			Space(2);
		}


		private Texture2D GetPreview (GameObject g) {
			if (PreviewTextureMap.ContainsKey(g)) {
				return PreviewTextureMap[g];
			} else {
				var result = AssetPreview.GetAssetPreview(g);
				if (result != null) {
					PreviewTextureMap.Add(g, result);
				}
				return result;
			}
		}


		private Rect ShrinkRect (Rect rect, float shrink) {
			rect.x += shrink;
			rect.y += shrink;
			rect.width -= shrink * 2f;
			rect.height -= shrink * 2f;
			return rect;
		}


		private void ShowPrefabPopMenu (List<int> prefabs, JujubePalette pal, int index) {
			if (prefabs == null || prefabs.Count == 0 || index < 0 || index >= prefabs.Count) { return; }
			var menu = new GenericMenu() { allowDuplicateNames = true, };
			int pIndex = prefabs[index];
			menu.AddDisabledItem(new GUIContent(pIndex >= 0 && pIndex < pal.Count ? $" {pal[pIndex].name} " : "-"), false);
			menu.AddItem(new GUIContent("Move Left"), false, () => {
				if (index > 0) {
					var temp = prefabs[index - 1];
					prefabs[index - 1] = prefabs[index];
					prefabs[index] = temp;
				}
			});
			menu.AddItem(new GUIContent("Move Right"), false, () => {
				if (index < prefabs.Count - 1) {
					var temp = prefabs[index + 1];
					prefabs[index + 1] = prefabs[index];
					prefabs[index] = temp;
				}
			});
			menu.AddItem(new GUIContent("Duplicate"), false, () => prefabs.Insert(index, pIndex));
			menu.AddItem(new GUIContent("Delete/Delete"), false, () => prefabs.RemoveAt(index));
			menu.AddSeparator(string.Empty);
			for (int i = 0; i < pal.Count; i++) {
				var prefabInPalette = pal[i];
				int _i = i;
				menu.AddItem(
					new GUIContent(prefabInPalette != null ? $"{_i}. {prefabInPalette.name}" : string.Empty),
					_i == pIndex,
					() => prefabs[index] = _i
				);
			}
			menu.ShowAsContext();
		}


		private void ShowItemPopMenu (JujubeGenerator jTarget, int index) {
			if (jTarget == null || jTarget.Items == null || index < 0 || index >= jTarget.Items.Count) { return; }
			var items = jTarget.Items;
			var menu = new GenericMenu() { allowDuplicateNames = true, };

			// Up
			if (index > 0) {
				menu.AddItem(new GUIContent("Move Up"), false, () => {
					var temp = items[index];
					items[index] = items[index - 1];
					items[index - 1] = temp;
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Move Up"));
			}

			// Down
			if (index < items.Count - 1) {
				menu.AddItem(new GUIContent("Move Down"), false, () => {
					var temp = items[index];
					items[index] = items[index + 1];
					items[index + 1] = temp;
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Move Down"));
			}

			// Delete
			menu.AddItem(new GUIContent("Delete/Delete"), false, () => items.RemoveAt(index));

			// Show
			menu.ShowAsContext();
		}


	}
}