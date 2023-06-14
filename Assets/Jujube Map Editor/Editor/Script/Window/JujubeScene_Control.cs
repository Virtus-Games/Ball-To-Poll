namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;
	using UnityEditorInternal;




	// === Control ===
	public partial class JujubeScene {



		// VAR
		private static bool Ctrl_Alt_Shift => Event.current.control || Event.current.alt || Event.current.shift;
		private static bool Ctrl_Alt => Event.current.control || Event.current.alt;
		private static bool Ctrl_Shift => Event.current.control || Event.current.shift;
		private static bool Alt_Shift => Event.current.alt || Event.current.shift;



		// System Menu
		[MenuItem("GameObject/3D Object/Jujube Map", priority = 0)]
		[MenuItem(JUJUBE_MENU_ROOT + "/New Map", priority = 0)]
		public static void CreateMap () {
			Selection.activeGameObject = new GameObject("New Jujube Map", typeof(JujubeRenderer));
		}



		[MenuItem(JUJUBE_MENU_ROOT + "/Edit Selecting", priority = 1)]
		public static void EditSelctingMap () {
			if (Selection.activeGameObject == null) { return; }
			var renderer = Selection.activeGameObject.GetComponent<JujubeRenderer>();
			if (renderer != null && renderer.Editable) {
				StartEdit(renderer);
			}
		}



		[MenuItem(JUJUBE_MENU_ROOT + "/Edit Selecting", priority = 1, validate = true)]
		public static bool EditSelctingMap_Validate () {
			if (Selection.activeGameObject == null) { return false; }
			var renderer = Selection.activeGameObject.GetComponent<JujubeRenderer>();
			return renderer != null && renderer.Editable;
		}



		[MenuItem(JUJUBE_MENU_ROOT + "/Stop Edit", priority = 2)]
		public static void MenuStopEdit () => EndEdit();



		[MenuItem(JUJUBE_MENU_ROOT + "/Stop Edit", priority = 2, validate = true)]
		public static bool MenuStopEdit_Validate () => EditingRenderer;



		[MenuItem(JUJUBE_MENU_ROOT + "/Clear Jujube Cache", priority = 3)]
		public static void ClearJujubeCache () {
			PaletteItemCacheMap.Clear();
		}



		[MenuItem(JUJUBE_MENU_ROOT + "/Setting", priority = 14)]
		public static void OpenWindow () {
			var window = EditorWindow.GetWindow<JujubeSettingWindow>(true, "Jujube Setting", true);
			window.minSize = new Vector2(390f, 480f);
			window.maxSize = new Vector2(390f, 480f);
			ClearBlockSelection();
			SceneView.RepaintAll();
		}



		// Value Fix
		private static void SceneGUI_ValueFix () {

			// Create Item when No Item
			if (EditingRenderer.Map.LayerCount == 0) {
				EditingRenderer.AddLayer();
				SetMapDirty(false);
				SetMapAssetDirty();
			}
			if (EditingRenderer.Palette.Count == 0) {
				EditingRenderer.Palette.AddItem(null);
				SetMapAssetDirty();
			}

			// Fix Index in Range
			SelectingLayerIndex = Mathf.Clamp(
				SelectingLayerIndex, 0, EditingRenderer.Map.LayerCount - 1
			);
			SelectingPaletteItemIndex = Mathf.Clamp(
				SelectingPaletteItemIndex, 0, EditingRenderer.Palette != null ? EditingRenderer.Palette.Count - 1 : 0
			);

			// Fix Renderer Scale to One
			if (!EditorUtil.Vector3Similar(EditingRenderer.transform.localScale, Vector3.one)) {
				EditingRenderer.transform.localScale = Vector3.one;
			}

			// Clear Selection when Not Using Select Tool
			if (SelectingBlockMap.Count > 0 && UsingTool != JujubeTool.Select && UsingTool != JujubeTool.Wand) {
				ClearSelection();
			}

			// Allow Rotate Painting Block
			var prefab = EditingRenderer.Palette[SelectingPaletteItemIndex];
			AllowRotatePaintingBlock = true;
			if (prefab != null) {
				var jBlock = prefab.GetComponent<JBlock>();
				if (jBlock != null && !jBlock.AllowRotate()) {
					AllowRotatePaintingBlock = false;
				}
			}
		}


		// Key
		private static void SceneGUI_Control_Before () {
			if (FocusingTextField) { return; }
			if (Event.current.type == EventType.KeyDown) {
				switch (Event.current.keyCode) {


					// Select All
					case KeyCode.A: {
						if (Alt_Shift || !Event.current.control) { break; }
						UsingTool = JujubeTool.Select;
						ClearSelection();
						bool overSelect = false;
						RefreshBlockShakeCache();
						int layerCount = EditingRenderer.Map.LayerCount;
						int startLayer = SelectingLayerIndex;
						int endLayer = SelectingLayerIndex;
						if (LayerMode == JujubeLayerMode.AllLayer) {
							startLayer = 0;
							endLayer = layerCount - 1;
						}
						for (int layerIndex = startLayer; layerIndex <= endLayer && layerIndex < layerCount && !overSelect; layerIndex++) {
							var layer = EditingRenderer.Map[layerIndex];
							if (layer == null || !layer.Visible) { continue; }
							int blockCount = layer.BlockCount;
							for (int blockIndex = 0; blockIndex < blockCount && !overSelect; blockIndex++) {
								var block = layer[blockIndex];
								if (block == null) { continue; }
								AddSelection(layerIndex, blockIndex, block);
								if (SelectingBlockMap.Count >= MaxSelectionCount.Value) {
									overSelect = true;
								}
							}
						}
						// Over Select
						if (overSelect) {
							LogTempWarningMessage($"Can Not Select More Than {MaxSelectionCount.Value} Blocks");
						}
						SceneView.RepaintAll();
						Event.current.Use();
						break;
					}


					// Copy Cut
					case KeyCode.X:
					case KeyCode.C: {
						if (Alt_Shift || !Event.current.control || SelectingBlockMap.Count == 0) { break; }
						// Copy
						bool RegistedUndo = false;
						CopyList.Clear();
						foreach (var pair in SelectingBlockMap) {
							if (pair.Value == null) { continue; }
							if (!RegistedUndo) {
								RegisterJujubeUndo();
								RegistedUndo = true;
							}
							CopyList.Add((pair.Value.Position, pair.Value.RotZ, pair.Value.Index));
						}
						// Cut
						if (Event.current.keyCode == KeyCode.X) {
							DeleteSelectingBlock();
						}
						SceneView.RepaintAll();
						Event.current.Use();
						break;
					}


					// Paste
					case KeyCode.V: {
						if (Alt_Shift || !Event.current.control || CopyList.Count == 0) { break; }
						ClearSelection();
						bool RegistedUndo = false;
						var palette = EditingRenderer.Palette;
						if (SelectingLayerIndex < 0 || SelectingLayerIndex >= EditingRenderer.Map.LayerCount) { break; }
						var layer = EditingRenderer.Map[SelectingLayerIndex];
						if (layer == null || !layer.Visible) { break; }
						foreach (var (position, rotationIndex, prefabIndex) in CopyList) {
							if (prefabIndex < 0 || prefabIndex >= palette.Count) { continue; }
							if (!RegistedUndo) {
								RegisterJujubeUndo();
								//SetJblockDirty();
								SetBlockCountDirty();
								RegistedUndo = true;
							}
							var block = EditingRenderer.AddBlock(
								layer,
								SelectingLayerIndex,
								prefabIndex,
								position,
								rotationIndex
							);
							AddSelection(
								SelectingLayerIndex,
								layer.BlockCount - 1,
								block
							);
						}
						SceneView.RepaintAll();
						Event.current.Use();
						break;
					}


					// Delete
					case KeyCode.Delete: {
						foreach (var obj in Selection.objects) {
							if (obj is GameObject gObj && EditorUtil.IsChildOf(gObj.transform, EditingRenderer.transform)) {
								Event.current.Use();
								break;
							}
						}
						if (Ctrl_Alt_Shift) { break; }
						DeleteSelectingBlock();
						SceneView.RepaintAll();
						Event.current.Use();
						break;
					}


					// Wand
					case KeyCode.Alpha2: {
						if (Ctrl_Alt_Shift) { break; }
						UsingTool = JujubeTool.Wand;
						SceneView.RepaintAll();
						Event.current.Use();
						break;
					}

				}
			}
		}


		private static void SceneGUI_Control (SceneView sceneView) {
			if (FocusingTextField) { return; }
			if (Event.current.type == EventType.KeyDown) {
				SceneGUI_Key_Tool();
				SceneGUI_Key_Layer();
				SceneGUI_Key_Palette();
				SceneGUI_Key_Paint(sceneView);
			}
		}


		private static void SceneGUI_Key_Layer () {
			switch (Event.current.keyCode) {

				// Select Up
				case KeyCode.Z: {
					if (Ctrl_Alt_Shift) { break; }
					SelectingLayerIndex = Mathf.Clamp(SelectingLayerIndex - 1, 0, EditingRenderer.Map.LayerCount - 1);
					ClearSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Select Down
				case KeyCode.X: {
					if (Ctrl_Alt_Shift) { break; }
					SelectingLayerIndex = Mathf.Clamp(SelectingLayerIndex + 1, 0, EditingRenderer.Map.LayerCount - 1);
					ClearSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Switch Visible
				case KeyCode.C: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					RegisterJujubeUndo();
					EditingRenderer.SetLayerVisible(
						SelectingLayerIndex,
						!EditingRenderer.GetLayerVisible(SelectingLayerIndex)
					);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

			}
		}


		private static void SceneGUI_Key_Palette () {
			switch (Event.current.keyCode) {
				case KeyCode.W: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					int newIndex = SelectingPaletteItemIndex - PaletteItemCountX;
					if (newIndex >= 0) {
						SelectingPaletteItemIndex = newIndex;
					}
					ScrollPaletteToSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.S: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					int newIndex = SelectingPaletteItemIndex + PaletteItemCountX;
					if (newIndex <= EditingRenderer.Palette.Count - 1) {
						SelectingPaletteItemIndex = newIndex;
					}
					ScrollPaletteToSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.A: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					if (SelectingPaletteItemIndex > SelectingPaletteItemIndex - SelectingPaletteItemIndex % PaletteItemCountX) {
						SelectingPaletteItemIndex--;
					}
					ScrollPaletteToSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.D: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					int paletteItemCountX = PaletteItemCountX;
					if (
						SelectingPaletteItemIndex < EditingRenderer.Palette.Count - 1 &&
						SelectingPaletteItemIndex < SelectingPaletteItemIndex - SelectingPaletteItemIndex % paletteItemCountX + paletteItemCountX - 1
					) {
						SelectingPaletteItemIndex++;
					}
					ScrollPaletteToSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

			}
		}


		private static void SceneGUI_Key_Paint (SceneView sceneView) {
			switch (Event.current.keyCode) {

				// Reload
				case KeyCode.F5: {
					if (Ctrl_Alt_Shift) { break; }
					ClearSelection();
					SetMapDirty(false);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Deselect
				case KeyCode.Escape: {
					if (Ctrl_Alt_Shift) { break; }
					ClearSelection();
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Move Blocks
				case KeyCode.W:
				case KeyCode.UpArrow: {
					if (Ctrl_Alt_Shift) { break; }
					MoveSelectingBlock(sceneView, 0, 1, false);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.S:
				case KeyCode.DownArrow: {
					if (Ctrl_Alt_Shift) { break; }
					MoveSelectingBlock(sceneView, 0, -1, false);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.A:
				case KeyCode.LeftArrow: {
					if (Ctrl_Alt_Shift) { break; }
					MoveSelectingBlock(sceneView, -1, 0, false);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.D:
				case KeyCode.RightArrow: {
					if (Ctrl_Alt_Shift) { break; }
					MoveSelectingBlock(sceneView, 1, 0, false);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.Q: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					MoveSelectingBlock(sceneView, 0, -1, true);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}
				case KeyCode.Minus: {
					if (Ctrl_Alt_Shift) { break; }
					MoveSelectingBlock(sceneView, 0, -1, true);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				case KeyCode.E: {
					if (Ctrl_Alt || !Event.current.shift) { break; }
					MoveSelectingBlock(sceneView, 0, 1, true);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}
				case KeyCode.Equals: {
					if (Ctrl_Alt_Shift) { break; }
					MoveSelectingBlock(sceneView, 0, 1, true);
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}
			}
		}


		private static void SceneGUI_Key_Tool () {
			switch (Event.current.keyCode) {


				// Select
				case KeyCode.Alpha1:
				case KeyCode.M: {
					if (Ctrl_Alt_Shift) { break; }
					UsingTool = JujubeTool.Select;
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Wand
				//case KeyCode.Alpha2:
				case KeyCode.N: {
					if (Ctrl_Alt_Shift) { break; }
					UsingTool = JujubeTool.Wand;
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Brush
				case KeyCode.Alpha3:
				case KeyCode.B: {
					if (Ctrl_Alt_Shift) { break; }
					UsingTool = JujubeTool.Brush;
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Erase
				case KeyCode.Alpha4:
				case KeyCode.R: {
					if (Ctrl_Alt_Shift) { break; }
					UsingTool = JujubeTool.Erase;
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Paint
				case KeyCode.Alpha5:
				case KeyCode.G: {
					if (Ctrl_Alt_Shift) { break; }
					UsingTool = JujubeTool.Paint;
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Pick
				case KeyCode.Alpha6:
				case KeyCode.P: {
					if (Ctrl_Alt_Shift) { break; }
					UsingTool = JujubeTool.Pick;
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}

				// Rotate Cursor/Selection
				case KeyCode.E:
				case KeyCode.Q: {
					if (Ctrl_Alt_Shift) { break; }
					bool positive = Event.current.keyCode == KeyCode.E;
					if (UsingTool == JujubeTool.Brush) {
						// Cursor
						if (!AllowRotatePaintingBlock) { break; }
						CursorRotationIndex.Value = Mathf.RoundToInt(Mathf.Repeat(
							CursorRotationIndex.Value + (positive ? 1f : -1f),
							CURSOR_ROTATION_LABELS.Length / 2
						));
						ClearCursorPrefab();
					} else if (SelectingBlockMap.Count > 0) {
						// Selection
						RotateSelectingBlocks(positive);
						RefreshMapBound();
					}
					SceneView.RepaintAll();
					Event.current.Use();
					break;
				}
			}
		}


		// Misc
		private static void DeleteSelectingBlock () {
			if (SelectingBlockMap.Count <= 0) { return; }
			bool undoRegisted = false;
			foreach (var pair in SelectingBlockMap) {
				if (!undoRegisted) {
					RegisterJujubeUndo();
					undoRegisted = true;
				}
				EditingRenderer.RemoveBlock(pair.Key.layerIndex, pair.Value);
			}
			RefreshMapBound();
			//SetJblockDirty();
			SetBlockCountDirty();
			ClearSelection();
		}


		private static void RotateSelectingBlocks (bool clockwise) {
			bool undoRegisted = false;
			var matrix = Matrix4x4.TRS(
				Vector3.zero,
				Quaternion.Euler(0f, clockwise ? 90f : -90f, 0f),
				Vector3.one
			);
			float cellSize = EditingRenderer.CellSize;
			var pivot = GetSelectingPivot();
			pivot.y = 0f;
			foreach (var pair in SelectingBlockMap) {
				var block = pair.Value;
				var blockTF = EditingRenderer.GetBlockTF(pair.Key.layerIndex, pair.Key.blockIndex);
				if (blockTF != null) {
					Vector3 pos = block.Position;
					pos -= pivot;
					pos = matrix.MultiplyPoint3x4(pos);
					pos += pivot;
					if (!undoRegisted) {
						RegisterJujubeUndo();
						SetMapAssetDirty();
						SelectingBlockMin = Vector3Int.one * short.MaxValue;
						SelectingBlockMax = Vector3Int.one * short.MinValue;
						undoRegisted = true;
					}
					block.Position = new Vector3Int(
						Mathf.RoundToInt(pos.x),
						Mathf.RoundToInt(pos.y),
						Mathf.RoundToInt(pos.z)
					);
					block.RotZ = Mathf.RoundToInt(Mathf.Repeat(block.RotZ + (clockwise ? 1 : -1), 4f));
					blockTF.localPosition = pos * cellSize;
					blockTF.localRotation = Quaternion.Euler(0f, block.RotZ * 90f, 0f);
				}
			}
		}


		private static void MoveSelectingBlock (SceneView sceneView, int screenX, int screenY, bool alt) {

			if (SelectingBlockMap.Count == 0) { return; }

			// Get Offset
			Vector3Int offset;
			if (!alt) {
				Vector3 normal = EditingRenderer.transform.worldToLocalMatrix.MultiplyPoint3x4(
					screenX > 0 ? sceneView.camera.transform.right :
					screenX < 0 ? -sceneView.camera.transform.right :
					screenY > 0 ? sceneView.camera.transform.forward :
					-sceneView.camera.transform.forward
				).normalized;
				normal.y = 0f;
				float minAngle = 360f;
				offset = Vector3Int.zero;
				Vector3Int[] OFFSETS = {
					new Vector3Int(0, 0, 1),
					new Vector3Int(1, 0, 1),
					new Vector3Int(1, 0, 0),
					new Vector3Int(1, 0, -1),
					new Vector3Int(0, 0, -1),
					new Vector3Int(-1, 0, -1),
					new Vector3Int(-1, 0, 0),
					new Vector3Int(-1, 0, 1),
				};
				for (int i = 0; i < OFFSETS.Length; i++) {
					float angle = Vector3.Angle(normal, OFFSETS[i]);
					if (angle < minAngle) {
						offset = OFFSETS[i];
						minAngle = angle;
					}
				}
			} else {
				offset = screenY > 0 ? Vector3Int.up : Vector3Int.down;
			}

			offset.y = Mathf.Max(offset.y, -SelectingBlockMin.y);
			if (offset == Vector3Int.zero) { return; }

			// Move Blocks
			bool undoRegisted = false;
			float cellSize = EditingRenderer.CellSize;
			SelectingBlockMin += offset;
			SelectingBlockMax += offset;
			if (SelectingPivot.HasValue) {
				SelectingPivot += offset;
			}
			foreach (var pair in SelectingBlockMap) {
				var block = pair.Value;
				var blockTF = EditingRenderer.GetBlockTF(pair.Key.layerIndex, pair.Key.blockIndex);
				if (blockTF != null) {
					if (!undoRegisted) {
						RegisterJujubeUndo();
						SetMapAssetDirty();
						RefreshMapBound();
						undoRegisted = true;
					}
					block.Position += offset;
					blockTF.localPosition = (Vector3)block.Position * cellSize;
				}
			}
		}


	}
}