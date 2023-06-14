namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;
	using UnityEditor.WindowsStandalone;



	// === Paint ===
	public partial class JujubeScene {


		// MSG
		private static void SceneGUI_Camera (SceneView sceneView) {

			var pivot = sceneView.pivot;
			if (LerpingCameraPivot.HasValue) {
				// Lerping
				pivot = Vector3.Lerp(pivot, LerpingCameraPivot.Value, 0.05f);
				sceneView.Repaint();
				if (Vector3.Distance(pivot, LerpingCameraPivot.Value) < 0.01f) {
					LerpingCameraPivot = null;
				}
			}

			// Clamp Pos
			var add = new Vector3(COORD_ADD, 0, COORD_ADD);
			var boundMax = MapLocalBounds.max;
			boundMax.y -= EditingRenderer.CellSize;
			var min = MapLocalBounds.min - add;
			var max = boundMax + add;
			var localPivot = EditingRenderer.transform.worldToLocalMatrix.MultiplyPoint3x4(pivot);
			localPivot.x = Mathf.Clamp(localPivot.x, min.x, max.x);
			localPivot.y = Mathf.Clamp(localPivot.y, min.y, max.y);
			localPivot.z = Mathf.Clamp(localPivot.z, min.z, max.z);
			pivot = EditingRenderer.transform.localToWorldMatrix.MultiplyPoint3x4(localPivot);
			if (!EditorUtil.Vector3Similar(pivot, sceneView.pivot)) {
				sceneView.pivot = pivot;
			}

			switch (Event.current.type) {
				case EventType.MouseDrag:
					LerpingCameraPivot = null;
					// Rotate
					if (Event.current.button == 1) {
						// Mosue Right Drag
						if (!Event.current.alt) {
							// View Rotate
							Vector2 del = Event.current.delta * 0.2f;
							float angle = sceneView.camera.transform.rotation.eulerAngles.x + del.y;
							angle = angle > 89 && angle < 180 ? 89 : angle;
							angle = angle > 180 && angle < 271 ? 271 : angle;
							sceneView.LookAt(
								sceneView.pivot,
								Quaternion.Euler(
									angle,
									sceneView.camera.transform.rotation.eulerAngles.y + del.x,
									0f
								),
								sceneView.size,
								sceneView.orthographic,
								true
							);
							Event.current.Use();
						}
					}
					break;
			}
		}


		private static void SceneGUI_Coordinate (SceneView sceneView, bool forward) {

			// Is Forward
			Vector3 rendererUp = EditingRenderer.transform.up;
			float cellSize = EditingRenderer.CellSize;
			float dis = new Plane(
				rendererUp,
				EditingRenderer.transform.position - rendererUp * cellSize / 2f
			).GetDistanceToPoint(sceneView.camera.transform.position);
			bool isForward = dis > 0f;
			if (isForward != forward) { return; }

			// Min Max
			const int COORD_MAX = 1024;
			var matrix = EditingRenderer.transform.localToWorldMatrix;
			float y = -cellSize / 2f;
			Vector3 min = new Vector3(Mathf.Min(MapLocalBounds.min.x, COORD_MAX * cellSize), 0f, Mathf.Min(MapLocalBounds.min.z, COORD_MAX * cellSize));
			Vector3 max = new Vector3(Mathf.Min(MapLocalBounds.max.x, COORD_MAX * cellSize), 0f, Mathf.Min(MapLocalBounds.max.z, COORD_MAX * cellSize));
			Vector3 minAlt = new Vector3(min.x - COORD_ADD * cellSize, 0f, min.z - COORD_ADD * cellSize);
			Vector3 maxAlt = new Vector3(max.x + COORD_ADD * cellSize, 0f, max.z + COORD_ADD * cellSize);

			// Alpha
			CoordAlpha = 0.8f * Mathf.Clamp01(Mathf.Abs(dis / 10f) - 0.05f);

			// Draw
			var oldC = Handles.color;
			var color = isForward ? COORD_COLOR : COORD_COLOR_BACK;
			color.a = CoordAlpha;
			Handles.color = color;
			DrawCoord(new Vector3(min.x, y, min.z), new Vector3(max.x, y, max.z), cellSize, matrix);

			color.a = CoordAlpha * (isForward ? 0.3f : 0.6f);
			Handles.color = color;
			DrawCoord(new Vector3(minAlt.x, y, minAlt.z), new Vector3(min.x, y, maxAlt.z), cellSize, matrix);
			DrawCoord(new Vector3(max.x, y, minAlt.z), new Vector3(maxAlt.x, y, maxAlt.z), cellSize, matrix);
			DrawCoord(new Vector3(min.x, y, minAlt.z), new Vector3(max.x, y, min.z), cellSize, matrix, false);
			DrawCoord(new Vector3(min.x, y, max.z), new Vector3(max.x, y, maxAlt.z), cellSize, matrix, false);

			if (isForward) {
				Vector3 pointMin, pointMax;

				// Draw Base Z
				color = COORD_COLOR_Z;
				color.a = CoordAlpha;
				pointMin = matrix.MultiplyPoint3x4(new Vector3(-0.5f * cellSize, y, min.z));
				pointMax = matrix.MultiplyPoint3x4(new Vector3(-0.5f * cellSize, y, maxAlt.z));
				Handles.color = color;
				Handles.DrawLine(pointMin, pointMax);
				Handles.ConeHandleCap(
					-1,
					pointMax,
					Quaternion.LookRotation(EditingRenderer.transform.forward, EditingRenderer.transform.up),
					HandleUtility.GetHandleSize(pointMax) * 0.2f,
					EventType.Repaint
				);

				// Draw Base X
				pointMin = matrix.MultiplyPoint3x4(new Vector3(min.x, y, -0.5f * cellSize));
				pointMax = matrix.MultiplyPoint3x4(new Vector3(maxAlt.x, y, -0.5f * cellSize));
				color = COORD_COLOR_X;
				color.a = CoordAlpha;
				Handles.color = color;
				Handles.DrawLine(pointMin, pointMax);
				Handles.ConeHandleCap(
					-1,
					pointMax,
					Quaternion.LookRotation(EditingRenderer.transform.right, EditingRenderer.transform.up),
					HandleUtility.GetHandleSize(pointMax) * 0.2f,
					EventType.Repaint
				);

			}

			// Done
			Handles.color = oldC;
		}


		private static void SceneGUI_MouseButtonCache (SceneView sceneView) {
			if (Event.current.isMouse) {
				bool isLeft = Event.current.button == 0;
				switch (Event.current.type) {
					case EventType.MouseDown:
						if (isLeft) {
							CurrentMouseLeftHolding = true;
							CurrentMouseLeftDragging = false;
						}
						CurrentMouseButton = Event.current.button;
						break;
					case EventType.MouseDrag:
						if (isLeft) {
							CurrentMouseLeftHolding = true;
							CurrentMouseLeftDragging = true;
						}
						CurrentMouseButton = Event.current.button;
						break;
					case EventType.MouseUp:
						if (isLeft) {
							CurrentMouseLeftHolding = false;
							CurrentMouseLeftDragging = false;
							if (!AllowRegisteUndoForMoveSelection) {
								AllowRegisteUndoForMoveSelection = true;
							}
						}
						CurrentMouseButton = Event.current.button;
						sceneView.Repaint();
						break;
					case EventType.MouseMove:
					case EventType.MouseEnterWindow:
					case EventType.MouseLeaveWindow:
						CurrentMouseLeftDragging = false;
						CurrentMouseLeftHolding = false;
						CurrentMouseButton = -1;
						Paint_CursorDownMapPosition = null;
						if (!AllowRegisteUndoForMoveSelection) {
							AllowRegisteUndoForMoveSelection = true;
						}
						break;
				}
			}
		}


		private static void SceneGUI_Cursor (SceneView sceneView) {

			// Refresh - Need Move Mouse To Paint Pos
			if (NeedMoveMouseToPaintPos.HasValue) {
				if (!Mathf.Approximately(NeedMoveMouseToPaintPos.Value.x, Event.current.mousePosition.x) ||
					!Mathf.Approximately(NeedMoveMouseToPaintPos.Value.y, Event.current.mousePosition.y)
				) {
					NeedMoveMouseToPaintPos = null;
				}
			}

			// Check - Mouse Button Check
			{
				bool returnCheck = false;
				if (CurrentMouseLeftHolding && CurrentMouseButton != 0) {
					returnCheck = true;
				}
				if (returnCheck || MouseInPanelGUI || !EditingRenderer.GetLayerVisible(SelectingLayerIndex)) {
					returnCheck = true;
				}
				if (returnCheck) {
					CursorPos = null;
					TrySetCursorPrefabActive(false, true);
					return;
				}
			}

			// Do it
			RaycastHit? closestHit = null;
			var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

			// Closest Block
			if (Physics.Raycast(ray, out RaycastHit _hit)) {
				if (EditingRenderer.IsBlockTF(_hit.transform)) {
					closestHit = _hit;
				}
			}

			// Cusor
			Vector3? cursorPos = null;
			Vector3 cursorNormal = default;
			float cellSize = EditingRenderer.CellSize;
			int cursorTintIndex = (int)UsingTool;
			bool wandShape = UsingTool == JujubeTool.Wand || (UsingTool == JujubeTool.Paint && PaintMode == JujubePaintMode.Bucket);
			bool painting = UsingTool == JujubeTool.Brush;
			bool picking = UsingTool == JujubeTool.Pick;
			bool isShapeCursor = false;
			bool mouseInJujubeHandle = MouseInJujubeHandle();
			if (closestHit.HasValue) {
				// Block-Based Cursor
				var hit = closestHit.Value;
				Vector3 pivotOffset = new Vector3(
					0f,
					EditingRenderer.PrefabPivot == JujubePrefabPivotMode.Top ? -cellSize / 2f :
					EditingRenderer.PrefabPivot == JujubePrefabPivotMode.Bottom ? cellSize / 2f :
					0f, 0f
				);
				cursorPos = hit.transform.position + (
					painting ? pivotOffset + hit.normal.normalized * cellSize : pivotOffset
				);
				cursorNormal = hit.normal.normalized;

				// Draw It
				if (!painting || !NeedMoveMouseToPaintPos.HasValue) {
					var cursorMapPos = GetBlockLocalPosition(cursorPos.Value);
					isShapeCursor = Paint_CursorDownMapPosition.HasValue && CurrentMouseLeftDragging && !picking && (!painting || Paint_CursorDownMapPosition != cursorMapPos);
					if (isShapeCursor) {
						DrawCursorShape(cursorMapPos, cellSize, cursorTintIndex, wandShape);
					}
					if (!mouseInJujubeHandle) {
						DrawCube(cursorPos.Value, hit.transform.rotation, cellSize, CURSOR_TINT[cursorTintIndex, 0], CoordAlpha);
					}
					if (!painting && !mouseInJujubeHandle) {
						DrawQuad(cursorPos.Value, hit.transform.up, hit.transform.forward, cursorNormal, cellSize, CURSOR_TINT[cursorTintIndex, 2], CoordAlpha);
					}
				}
			} else if (!picking) {
				Vector3 rendererUp = EditingRenderer.transform.up;
				float dis = new Plane(
					rendererUp,
					EditingRenderer.transform.position - rendererUp * cellSize / 2f
				).GetDistanceToPoint(sceneView.camera.transform.position);
				if (dis > 0f) {
					// Coord-Based Cursor
					var coordUp = EditingRenderer.transform.up;
					var coordPlane = new Plane(coordUp, EditingRenderer.transform.position - cellSize * 0.5f * coordUp);
					if (coordPlane.Raycast(ray, out float enter)) {
						var point = ray.GetPoint(enter);
						var localPoint = EditingRenderer.transform.worldToLocalMatrix.MultiplyPoint3x4(point);
						localPoint.x = EditorUtil.Snap(localPoint.x, 1f / cellSize);
						localPoint.y = EditorUtil.Snap(localPoint.y + cellSize * 0.5f, 1f / cellSize);
						localPoint.z = EditorUtil.Snap(localPoint.z, 1f / cellSize);
						point = EditingRenderer.transform.localToWorldMatrix.MultiplyPoint3x4(localPoint);
						cursorPos = point;
						cursorNormal = EditingRenderer.transform.up.normalized;
						// Draw It
						if (!painting || !NeedMoveMouseToPaintPos.HasValue) {
							var cursorMapPos = GetBlockLocalPosition(cursorPos.Value);
							isShapeCursor = Paint_CursorDownMapPosition.HasValue && CurrentMouseLeftDragging && (!painting || Paint_CursorDownMapPosition != cursorMapPos);
							if (isShapeCursor) {
								DrawCursorShape(cursorMapPos, cellSize, cursorTintIndex, wandShape);
							}
							if (!mouseInJujubeHandle) {
								if (painting) {
									DrawCube(cursorPos.Value, EditingRenderer.transform.rotation, cellSize, CURSOR_TINT[cursorTintIndex, 0], CoordAlpha);
								} else {
									DrawQuad(cursorPos.Value - cursorNormal * cellSize, cursorNormal, EditingRenderer.transform.forward, cursorNormal, cellSize, CURSOR_TINT[cursorTintIndex, 2], CoordAlpha);
								}
							}
						}
					}
				}
			}

			// Repaint
			if (cursorPos.HasValue != CursorPos.HasValue) {
				sceneView.Repaint();
			} else if (cursorPos.HasValue && (!EditorUtil.Vector3Similar(cursorPos.Value, CursorPos.Value) || !EditorUtil.Vector3Similar(cursorNormal, CursorNormal))) {
				sceneView.Repaint();
			}

			CursorPos = cursorPos;
			CursorNormal = cursorNormal;

			// Prefab Cursor
			bool hasPrefabCursor = !isShapeCursor && CursorPos.HasValue && painting && !NeedMoveMouseToPaintPos.HasValue;
			if (hasPrefabCursor) {
				EditingRoot_Cursor.position = CursorPos.Value;
				EditingRoot_Cursor.rotation = EditingRenderer.transform.rotation;
				EditingRoot_Cursor.localScale = Vector3.one;
				GameObject prefab = null;
				if (EditingRenderer.Palette && SelectingPaletteItemIndex >= 0 && SelectingPaletteItemIndex < EditingRenderer.Palette.Count) {
					prefab = EditingRenderer.Palette[SelectingPaletteItemIndex];
				}
				if (prefab != null) {
					if (EditingRoot_Cursor.childCount > 0) {
						string _name = EditingRoot_Cursor.GetChild(0).gameObject.name;
						if (_name != prefab.GetInstanceID().ToString()) {
							EditorUtil.DestroyAllChirldrenImmediate(EditingRoot_Cursor);
						}
					}
					if (EditingRoot_Cursor.childCount == 0) {
						var pivotMode = EditingRenderer.PrefabPivot;
						var cursorTF = Object.Instantiate(prefab.gameObject, EditingRoot_Cursor).transform;
						cursorTF.gameObject.name = prefab.GetInstanceID().ToString();
						cursorTF.gameObject.SetActive(true);
						cursorTF.localPosition = Vector3.up * (
							pivotMode == JujubePrefabPivotMode.Top ? cellSize / 2f :
							pivotMode == JujubePrefabPivotMode.Bottom ? -cellSize / 2f :
							0f
						);
						cursorTF.localRotation = Quaternion.Euler(0f,
							CursorRotationIndex.Value * 90f
						, 0f);
						cursorTF.localScale = EditingRenderer.GetPrefabLocalScale(prefab.transform);
						EditorUtil.SetHideFlagForAllChildren(cursorTF, HideFlags.HideAndDontSave);
					}
				} else if (EditingRoot_Cursor.childCount > 0) {
					EditorUtil.DestroyAllChirldrenImmediate(EditingRoot_Cursor);
				}
			}
			TrySetCursorPrefabActive(hasPrefabCursor, true);
		}


		private static void SceneGUI_Paint () {

			if (!CursorPos.HasValue || CurrentMouseButton != 0) { return; }
			if (SelectingLayerIndex < 0 || SelectingLayerIndex >= EditingRenderer.Map.LayerCount) { return; }
			if (SelectingPaletteItemIndex < 0 || SelectingPaletteItemIndex >= EditingRenderer.Palette.Count) { return; }
			if (!EditingRenderer.GetLayerVisible(SelectingLayerIndex)) { return; }

			// Calculate Cursor Map Pos
			var cursorMapPos = GetBlockLocalPosition(CursorPos.Value);

			// Down Drag Logic
			bool paintNow = false;
			if (Event.current.button == 0) {
				switch (Event.current.type) {
					default:
						return;
					case EventType.MouseDown:
						Paint_CursorDownMapPosition = cursorMapPos;
						if (!Event.current.control && SelectingBlockMap.Count > 0) {
							ClearSelection();
						}
						break;
					case EventType.MouseDrag:
						break;
					case EventType.MouseUp:
						paintNow = true;
						break;
				}
			}

			if (NeedMoveMouseToPaintPos.HasValue) { return; }

			// Paint Now
			if (paintNow && Paint_CursorDownMapPosition.HasValue) {
				var min = Vector3Int.Min(Paint_CursorDownMapPosition.Value, cursorMapPos);
				var max = Vector3Int.Max(Paint_CursorDownMapPosition.Value, cursorMapPos);

				// Wand Mode
				JujubeWandMode? wandMode = null;
				if (UsingTool == JujubeTool.Wand) {
					wandMode = WandMode;
				} else if (UsingTool == JujubeTool.Paint && PaintMode == JujubePaintMode.Bucket) {
					wandMode = JujubeWandMode.Same;
				}

				switch (UsingTool) {
					case JujubeTool.Select:
					case JujubeTool.Wand:
						Perform_Selection(min, max, cursorMapPos, wandMode);
						break;

					case JujubeTool.Brush:
						Perform_Brush(min, max, cursorMapPos, wandMode);
						break;

					case JujubeTool.Paint:
						Perform_Paint(min, max, cursorMapPos, wandMode);
						break;

					case JujubeTool.Erase:
						Perform_Erase(min, max, cursorMapPos, wandMode);
						break;

					case JujubeTool.Pick:
						Perform_Pick(cursorMapPos);
						break;

				}
				Event.current.Use();
			}
		}


		private static void SceneGUI_Focus () {
			if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && Event.current.shift) {
				RaycastHit? hit = null;
				var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

				// Hitted Block
				if (Physics.Raycast(ray, out RaycastHit _hit)) {
					if (EditingRenderer.IsBlockTF(_hit.transform)) {
						hit = _hit;
					}
				}

				// Focus
				if (hit.HasValue) {
					// Block
					SetLerpingCameraPivot(hit.Value.point);
				} else {
					// Coord
					var coordUp = EditingRenderer.transform.up;
					var coordPlane = new Plane(coordUp, EditingRenderer.transform.position - EditingRenderer.CellSize * 0.5f * coordUp);
					if (coordPlane.Raycast(ray, out float enter)) {
						SetLerpingCameraPivot(ray.GetPoint(enter));
					}
				}
			}
		}


		// Paint Logic
		private static void Perform_Selection (Vector3Int min, Vector3Int max, Vector3Int cursorMapPos, JujubeWandMode? wandMode) {
			// Clear Selection
			if (!Ctrl_Shift) {
				ClearSelection();
			}
			// Add Selection
			int targetLayerIndex = LayerMode == JujubeLayerMode.CurrentLayer ? SelectingLayerIndex : -1;
			RefreshBlockShakeCache();

			PerformWithShape(min, max, cursorMapPos, MaxSelectionCount.Value, wandMode, (pos) => {
				(int _layerIndex, int index, _) = EditingRenderer.GetBlockIndex(pos, targetLayerIndex, true);
				if (index < 0) { return false; }
				var layer = EditingRenderer.Map[_layerIndex];
				if (layer == null) { return false; }
				if (Event.current.control && Event.current.shift) {
					RemoveSelection(_layerIndex, index);
				} else {
					AddSelection(_layerIndex, index, layer[index]);
				}
				return true;
			}, () => LogTempWarningMessage($"Can Not Select More Than {MaxSelectionCount.Value} Blocks"));

		}


		private static void Perform_Brush (Vector3Int min, Vector3Int max, Vector3Int cursorMapPos, JujubeWandMode? wandMode) {
			bool undoRegisted = false;

			PerformWithShape(min, max, cursorMapPos, MaxPaintCount.Value, wandMode, (pos) => {
				if (EditingRenderer.GetBlockIndex(pos, -1, true).blockIndex >= 0) { return false; }
				if (!undoRegisted) {
					RegisterJujubeUndo();
					SetMapAssetDirty();
					//SetJblockDirty();
					SetBlockCountDirty();
					undoRegisted = true;
				}
				EditingRenderer.AddBlock(
					SelectingLayerIndex,
					SelectingPaletteItemIndex,
					pos,
					AllowRotatePaintingBlock ? CursorRotationIndex.Value : 0
				);
				return true;
			}, () => LogTempWarningMessage($"Can Not Spawn More Than {MaxPaintCount.Value} Blocks At Once"));
			NeedMoveMouseToPaintPos = Event.current.mousePosition;
			if (undoRegisted) {
				RefreshMapBound();
			}
		}


		private static void Perform_Paint (Vector3Int min, Vector3Int max, Vector3Int cursorMapPos, JujubeWandMode? wandMode) {

			if (SelectingPaletteItemIndex < 0 || SelectingPaletteItemIndex >= EditingRenderer.Palette.Count) { return; }
			int targetLayerIndex = LayerMode == JujubeLayerMode.CurrentLayer ? SelectingLayerIndex : -1;
			bool undoRegisted = false;

			PerformWithShape(min, max, cursorMapPos, MaxPaintCount.Value, wandMode, (pos) => {
				var (_layerIndex, index, block) = EditingRenderer.GetBlockIndex(pos, targetLayerIndex, true);
				if (block == null) { return false; }
				if (!undoRegisted) {
					RegisterJujubeUndo();
					SetMapAssetDirty();
					//SetJblockDirty();
					undoRegisted = true;
				}
				block.Index = SelectingPaletteItemIndex;
				EditingRenderer.RespawnBlock(_layerIndex, index, block);
				return true;
			}, () => LogTempWarningMessage($"Can Not Paint More Than {MaxPaintCount.Value} Blocks At Once"));

		}


		private static void Perform_Erase (Vector3Int min, Vector3Int max, Vector3Int cursorMapPos, JujubeWandMode? wandMode) {
			bool undoRegisted = false;

			PerformWithShape(min, max, cursorMapPos, int.MaxValue, wandMode, (pos) => {
				(int _layerIndex, int index, _) = EditingRenderer.GetBlockIndex(pos, -1, true);
				if (index < 0) { return false; }
				if (LayerMode == JujubeLayerMode.CurrentLayer && _layerIndex != SelectingLayerIndex) { return false; }
				if (!undoRegisted) {
					RegisterJujubeUndo();
					SetMapAssetDirty();
					//SetJblockDirty();
					SetBlockCountDirty();
					undoRegisted = true;
				}
				EditingRenderer.RemoveBlock(_layerIndex, index);
				return true;
			});

			if (undoRegisted) {
				RefreshMapBound();
			}
		}


		private static void Perform_Pick (Vector3Int cursorMapPos) {
			if (cursorMapPos.y < 0) { return; }
			(int _layerIndex, int index, var block) = EditingRenderer.GetBlockIndex(cursorMapPos, -1, true);
			if (index < 0) { return; }
			var layer = EditingRenderer.Map[_layerIndex];
			if (layer == null) { return; }
			SelectingPaletteItemIndex = Mathf.Clamp(block.Index, 0, EditingRenderer.Palette.Count - 1);
		}


		// LGC
		private static void ClearCursorPrefab () {
			if (EditingRoot_Cursor != null) {
				EditorUtil.DestroyAllChirldrenImmediate(EditingRoot_Cursor);
			}
		}


		private static void TrySetCursorPrefabActive (bool active, bool repaint = false) {
			if (EditingRoot_Cursor != null) {
				bool needRepaint = false;
				if (active != EditingRoot_Cursor.gameObject.activeSelf) {
					EditingRoot_Cursor.gameObject.SetActive(active);
					needRepaint = true;
				}
				if (repaint && needRepaint) {
					SceneView.RepaintAll();
				}
			}
		}


		private static Vector3Int GetBlockLocalPosition (Vector3 worldPos) {
			float cellSize = EditingRenderer.CellSize;
			var cursorMapPos_Vector3 = EditingRenderer.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPos);
			cursorMapPos_Vector3.x /= cellSize;
			cursorMapPos_Vector3.y /= cellSize;
			cursorMapPos_Vector3.z /= cellSize;
			var cursorMapPos = new Vector3Int(
				Mathf.RoundToInt(cursorMapPos_Vector3.x),
				Mathf.RoundToInt(cursorMapPos_Vector3.y),
				Mathf.RoundToInt(cursorMapPos_Vector3.z)
			);
			return cursorMapPos;
		}


		// Shape
		private static void DrawCursorShape (Vector3Int cursorMapPos, float cellSize, int cursorTintIndex, bool wandShape) {
			if (!wandShape) {
				// Box Shape
				Vector3 min = Vector3Int.Min(Paint_CursorDownMapPosition.Value, cursorMapPos);
				Vector3 max = Vector3Int.Max(Paint_CursorDownMapPosition.Value, cursorMapPos);
				min.y = Mathf.Max(min.y, 0);
				max.y = Mathf.Max(max.y, 0);
				min *= cellSize;
				max *= cellSize;
				min -= cellSize * 0.5f * Vector3.one;
				max += cellSize * 0.5f * Vector3.one;
				DrawWireCube(min, max, EditingRenderer.transform.localToWorldMatrix, CURSOR_TINT[cursorTintIndex, 1], CoordAlpha);
			} else {
				// Wand Shape
				DrawWireCube((Vector3)cursorMapPos * cellSize - cellSize * 0.5f * Vector3.one, (Vector3)cursorMapPos * cellSize + cellSize * 0.5f * Vector3.one, EditingRenderer.transform.localToWorldMatrix, CURSOR_TINT[cursorTintIndex, 1], CoordAlpha);
			}
		}


		private static void PerformWithShape (Vector3Int min, Vector3Int max, Vector3Int cursorPos, int maxPerformCount, JujubeWandMode? wandMode, System.Func<Vector3Int, bool> perform, System.Action overDoneCallback = null) {
			bool overDone = false;
			int doneCount = 0;
			if (!wandMode.HasValue) {
				// Box Shape
				for (int y = min.y; y <= max.y && !overDone; y++) {
					if (y < 0) { continue; }
					for (int x = min.x; x <= max.x && !overDone; x++) {
						for (int z = min.z; z <= max.z && !overDone; z++) {
							if (perform(new Vector3Int(x, y, z))) {
								doneCount++;
							}
							overDone = doneCount >= maxPerformCount;
						}
					}
				}
				if (overDone && overDoneCallback != null) {
					overDoneCallback();
				}

			} else {
				// Wand Shape
				if (cursorPos.y < 0) { return; }
				int targetLayerIndex = LayerMode == JujubeLayerMode.AllLayer ? -1 : SelectingLayerIndex;
				var block = EditingRenderer.GetBlockIndex(cursorPos, targetLayerIndex, true).block;
				if (block == null) { return; }
				int magicIndex = block.Index;
				Vector3Int _pos;
				Vector3Int _p = Vector3Int.zero;
				JujubeBlock _block;
				var stack = new Stack<Vector3Int>();
				var doneHash = new HashSet<Vector3Int>();
				stack.Push(cursorPos);
				doneHash.Add(cursorPos);
				int absX, absY, absZ, absSum;
				bool noDiagonal = WandSpreadMode != JujubeWandSpreadMode.StraightAndDiagonal;
				while (stack.Count > 0 && doneCount < maxPerformCount) {
					_pos = stack.Pop();
					if (perform(_pos)) {
						doneCount++;
					}
					for (int x = -1; x <= 1; x++) {
						for (int y = -1; y <= 1; y++) {
							for (int z = -1; z <= 1; z++) {
								absX = Mathf.Abs(x);
								absY = Mathf.Abs(y);
								absZ = Mathf.Abs(z);
								absSum = absX + absY + absZ;
								if (absSum == 0 || absSum == 3 || (noDiagonal && absSum == 2)) { continue; }
								_p.x = _pos.x + x;
								_p.y = _pos.y + y;
								_p.z = _pos.z + z;
								if (!doneHash.Contains(_p)) {
									_block = EditingRenderer.GetBlockIndex(_p, targetLayerIndex, true).block;
									if (_block != null && (wandMode.Value == JujubeWandMode.Any || _block.Index == magicIndex)) {
										stack.Push(_p);
										doneHash.Add(_p);
									}
								}
							}
						}
					}
				}
				if (doneCount >= maxPerformCount && overDoneCallback != null) {
					overDoneCallback();
				}
			}
		}


	}
}