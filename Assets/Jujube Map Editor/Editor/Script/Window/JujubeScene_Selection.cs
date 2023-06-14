namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;



	// === Selection ===
	public partial class JujubeScene {




		#region --- MSG ---


		private static void SceneGUI_Selection (SceneView sceneView) {
			if (SelectingBlockMap.Count == 0) { return; }

			float cellSize = EditingRenderer.CellSize;
			Vector3 halfSell = 0.5f * cellSize * Vector3.one;
			var localToWorldMatrix = EditingRenderer.transform.localToWorldMatrix;
			var worldToLocalMatrix = EditingRenderer.transform.worldToLocalMatrix;

			// Min Max Frame
			if (SelectingBlockMin.x > SelectingBlockMax.x) {
				foreach (var pair in SelectingBlockMap) {
					var block = pair.Value;
					if (block == null) { continue; }
					SelectingBlockMin = Vector3Int.Min(SelectingBlockMin, block.Position);
					SelectingBlockMax = Vector3Int.Max(SelectingBlockMax, block.Position);
					//RefreshSelectingPivot();
				}
			}

			// Draw Wire Cube
			if (SelectingBlockMin.x <= SelectingBlockMax.x) {
				DrawWireCube(
					(Vector3)SelectingBlockMin * cellSize - halfSell,
					(Vector3)SelectingBlockMax * cellSize + halfSell,
					localToWorldMatrix,
					SELECTING_BLOCK_TINT, SELECTING_BLOCK_TINT.a, true
				);
			}

			// In Range But Not Selecting Blocks
			bool selectCurrentLayer = LayerMode == JujubeLayerMode.CurrentLayer;
			for (int i = 0; i < EditingRenderer.Map.Layers.Count; i++) {
				var layer = EditingRenderer.Map.Layers[i];
				if (layer == null || !layer.Visible || (selectCurrentLayer && i != SelectingLayerIndex)) { continue; }
				for (int j = 0; j < layer.Blocks.Count; j++) {
					var block = layer.Blocks[j];
					if (
						block == null ||
						SelectingBlockMap.ContainsKey((i, j)) ||
						!EditorUtil.InRange(block.Position, SelectingBlockMin, SelectingBlockMax)
					) { continue; }
					// Draw Frame
					DrawWireCube(
						(Vector3)block.Position * cellSize - halfSell,
						(Vector3)block.Position * cellSize + halfSell,
						localToWorldMatrix,
						SELECTING_BLOCK_TINT_ALT, SELECTING_BLOCK_TINT_ALT.a, false
					);
				}
			}

			// Move Handle
			Vector3 oldLocalPos = 0.5f * (Vector3)(SelectingBlockMin + SelectingBlockMax);
			Vector3 oldPos = localToWorldMatrix.MultiplyPoint3x4(cellSize * oldLocalPos);
			Vector3 newPos = oldPos;

			switch (PositionHandleType) {
				case JujubePositionHandleType.Quad:
					newPos = QuadPositionHandle(
						oldPos,
						SelectingBlockMin,
						SelectingBlockMax,
						EditingRenderer.transform.forward.normalized,
						EditingRenderer.transform.up.normalized,
						EditingRenderer.transform.right.normalized,
						cellSize,
						sceneView.camera.transform.position,
						BOX_POS_HANDLE_COLORS[0],
						BOX_POS_HANDLE_COLORS[1]
					);
					break;
				case JujubePositionHandleType.Box:
					newPos = BoxPositionHandle(
						oldPos,
						SelectingBlockMin,
						SelectingBlockMax,
						EditingRenderer.transform.forward.normalized,
						EditingRenderer.transform.up.normalized,
						EditingRenderer.transform.right.normalized,
						cellSize,
						sceneView.camera.transform.position,
						BOX_POS_HANDLE_COLORS[0],
						BOX_POS_HANDLE_COLORS[1]
					);
					break;
				case JujubePositionHandleType.Arrow:
					newPos = Handles.DoPositionHandle(oldPos, EditingRenderer.transform.rotation);
					break;
			}

			var newLocalPos = worldToLocalMatrix.MultiplyPoint3x4(newPos) / cellSize;
			var localOffset = new Vector3Int(
				Mathf.RoundToInt(newLocalPos.x - oldLocalPos.x),
				Mathf.RoundToInt(newLocalPos.y - oldLocalPos.y),
				Mathf.RoundToInt(newLocalPos.z - oldLocalPos.z)
			);
			// Clamp
			localOffset.y = Mathf.Max(localOffset.y, -SelectingBlockMin.y);
			if (localOffset != Vector3Int.zero) {
				// Undo 
				if (AllowRegisteUndoForMoveSelection) {
					RegisterJujubeUndo();
					//SetJblockDirty();
					AllowRegisteUndoForMoveSelection = false;
				}
				// Min Max
				SelectingBlockMin += localOffset;
				SelectingBlockMax += localOffset;
				SelectingPivot += localOffset;
				// Blocks
				foreach (var pair in SelectingBlockMap) {
					(int layerIndex, int blockIndex) = pair.Key;
					(var block, var blockTF) = EditingRenderer.GetBlockAndBlockTF(layerIndex, blockIndex);
					if (block == null || blockTF == null) { continue; }
					block.Position += localOffset;
					blockTF.localPosition = (Vector3)block.Position * cellSize;
				}
				// End
				RefreshMapBound();
			}

		}


		#endregion




		#region --- LGC ---


		private static void AddSelection (int layerIndex, int blockIndex, JujubeBlock block) {
			if (block == null ||
				SelectingBlockMap.Count >= MaxSelectionCount.Value ||
				SelectingBlockMap.ContainsKey((layerIndex, blockIndex))
			) { return; }
			SelectingBlockMap.Add((layerIndex, blockIndex), block);
			SelectingBlockMin = Vector3Int.Min(SelectingBlockMin, block.Position);
			SelectingBlockMax = Vector3Int.Max(SelectingBlockMax, block.Position);
			SelectingPivot = null;
		}


		private static void RemoveSelection (int layerIndex, int blockIndex) {
			if (!SelectingBlockMap.ContainsKey((layerIndex, blockIndex))) { return; }
			SelectingBlockMap.Remove((layerIndex, blockIndex));
			SelectingBlockMin = Vector3Int.one * short.MaxValue;
			SelectingBlockMax = Vector3Int.one * short.MinValue;
		}


		private static void ClearSelection () {
			SelectingBlockMap.Clear();
			SelectingBlockMin = Vector3Int.one * short.MaxValue;
			SelectingBlockMax = Vector3Int.one * short.MinValue;
			SelectingPivot = null;
		}


		private static Vector3 GetSelectingPivot () {
			if (!SelectingPivot.HasValue) {
				Vector3 pivot = (Vector3)(SelectingBlockMin + SelectingBlockMax) * 0.5f;
				if (Mathf.Approximately(pivot.x % 1f, pivot.z % 1f)) {
					SelectingPivot = pivot;
				} else {
					SelectingPivot = new Vector3(
						Mathf.Floor(pivot.x),
						Mathf.Floor(pivot.y),
						Mathf.Floor(pivot.z)
					);
				}
			}
			return SelectingPivot.Value;
		}


		#endregion




	}
}