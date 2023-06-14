namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;



	// === Panel ===
	public partial class JujubeScene {




		#region --- MSG ---


		private static void SceneGUI_Panel (SceneView sceneView) {
			var rect = new Rect(6, 6, PANEL_PADDING + PANEL_WIDTH + PANEL_PADDING, sceneView.position.height);
			var panelRect = rect;
			HandlesLayoutV(rect, () => {

				// Layer
				SceneGUI_Panel_Layer();
				Space(2);

				// Palette
				ColorBlock(GUIRect(0, 1.5f), new Color(0f, 0f, 0f, 0.15f));
				SceneGUI_Panel_Palette();

				Space(1);

				// Bottom Bar
				ColorBlock(GUIRect(0, 1.5f), new Color(0f, 0f, 0f, 0.15f));
				LayoutH(() => {
					// Done
					var doneRect = GUIRect(0, PANEL_TITLE_HEIGHT);
					panelRect.height = doneRect.y + doneRect.height;
					if (GUI.Button(doneRect, "Done", EditorStyles.toolbarButton)) {
						EditorApplication.delayCall += EndEdit;
					}
				}, false, EditorStyles.toolbar);

				// Drag
				SceneGUI_Panel_Drag(panelRect);

			}, PanelShadowBoxStyle);
			if (Event.current.isMouse) {
				MouseInPanelGUI = panelRect.Contains(Event.current.mousePosition);
			}

		}


		private static void SceneGUI_Panel_Layer () {

			// Title Bar
			LayoutH(() => {
				// Map Switcher
				if (GUI.Button(GUIRect(0, PANEL_TITLE_HEIGHT), new GUIContent($" {EditingRenderer.name} ", GetJujubeImage("Icon Green.png")), EditorStyles.toolbarPopup)) {
					ShowMapSelectorPopupMenu();
				}
				// Close Button
				if (GUI.Button(GUIRect(24, PANEL_TITLE_HEIGHT), "×", EditorStyles.toolbarButton)) {
					EditorApplication.delayCall += EndEdit;
				}
			}, false, EditorStyles.toolbar);
			Space(2);

			// Content
			LayerPanelScrollPosition = EditorGUILayout.BeginScrollView(
				LayerPanelScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar,
				GUILayout.Height(LAYER_CONTENT_HEIGHT)
			);

			bool mouseDown = Event.current.type == EventType.MouseDown;
			int downIndex = -1;
			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < EditingRenderer.Map.LayerCount; i++) {
				var layer = EditingRenderer.Map[i];
				bool selecting = i == SelectingLayerIndex;
				LayoutH(() => {

					Space(2);
					var labelRect = GUIRect(20, PANEL_ITEM_HEIGHT);
					var rect = GUIRect(0, PANEL_ITEM_HEIGHT);
					var bgRect = new Rect(0f, rect.y - 1, PANEL_WIDTH, rect.height + 2);

					// BG
					ColorBlock(bgRect,
						selecting ? HIGHLIGHT_TINT :
						i % 2 == 0 ?
						new Color(1f, 1f, 1f, 0.03f) :
						new Color(0f, 0f, 0f, 0.06f)
					);

					// Index
					GUI.Label(labelRect, $"{i}.");

					// Layer Name
					layer.LayerName = TextField(
						selecting ? rect : new Rect(),
						layer.LayerName,
						ref FocusingTextField,
						GUI.skin.label
					);
					GUI.Label(selecting ? new Rect() : rect, layer.LayerName, GUI.skin.label);
					Space(2);

					// Visible
					bool layerVisible = EditingRenderer.GetLayerVisible(i);
					bool newLayerVisible = GUI.Toggle(
						GUIRect(PANEL_ITEM_HEIGHT, PANEL_ITEM_HEIGHT),
						layerVisible,
						GUIContent.none
					);
					if (layerVisible != newLayerVisible) {
						EditingRenderer.SetLayerVisible(i, newLayerVisible);
					}
					Space(2);

					// Mouse Down
					if (mouseDown && downIndex < 0 && rect.Contains(Event.current.mousePosition)) {
						downIndex = i;
					}
				});
				Space(2);
			}
			Space(2);
			if (EditorGUI.EndChangeCheck()) {
				RegisterJujubeUndo();
				SetMapAssetDirty();
			}

			// Mouse Down
			if (downIndex >= 0) {
				if (Event.current.button == 0) {
					SelectingLayerIndex = downIndex;
					ClearSelection();
				} else if (Event.current.button == 1) {
					ShowLayerItemMenu(downIndex);
					GUI.FocusControl("");
				}
				Event.current.Use();
			}

			// Add Layer
			LayoutH(() => {
				GUIRect(0, PANEL_ITEM_HEIGHT);
				if (GUI.Button(GUIRect(72, PANEL_ITEM_HEIGHT), "+ Layer")) {
					RegisterJujubeUndo();
					EditingRenderer.AddLayer();
					SetMapAssetDirty();
				}
				Space(4);
			});
			Space(36);


			// End
			EditorGUILayout.EndScrollView();
		}


		private static void SceneGUI_Panel_Palette () {

			var palette = EditingRenderer.Palette;
			if (palette == null) { return; }
			int count = palette.Count;
			int itemCountH = PaletteItemCountX;
			int itemSize = (PANEL_WIDTH - 14) / itemCountH;
			int itemCountV = Mathf.CeilToInt((float)count / itemCountH);


			// Title Bar
			LayoutH(() => {

				GUIRect(0, PANEL_TITLE_HEIGHT);

				// Setting Button
				if (GUI.Button(GUIRect(24, PANEL_TITLE_HEIGHT), new GUIContent(GetJujubeImage(EditorGUIUtility.isProSkin ? "Setting_pro.png" : "Setting_per.png")), EditorStyles.toolbarButton)) {
					EditorApplication.delayCall += OpenWindow;
				}

				// State Button
				if (GUI.Button(GUIRect(24, PANEL_TITLE_HEIGHT), "≡", EditorStyles.toolbarButton)) {
					ShowStateLabel.Value = !ShowStateLabel.Value;
				}

				// Add
				if (GUI.Button(GUIRect(24, PANEL_TITLE_HEIGHT), "+", EditorStyles.toolbarButton)) {
					EditorApplication.delayCall += () => PickPalettePrefab();
				}

			}, false, EditorStyles.toolbar);
			Space(2);

			// Content
			PalettePanelScrollPosition = EditorGUILayout.BeginScrollView(
				PalettePanelScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar,
				GUILayout.Height(PALETTE_CONTENT_HEIGHT)
			);

			// Content
			Rect rect = default;
			int index = 0;
			int downIndex = -1;
			bool mouseDown = Event.current.type == EventType.MouseDown && Event.current.button <= 1;
			for (int i = 0; i < itemCountV && index < count; i++) {
				LayoutH(() => {
					for (int j = 0; j < itemCountH && index < count; j++) {
						rect = GUIRect(itemSize, itemSize);

						// Draw Highlight
						if (SelectingPaletteItemIndex == index) {
							ColorBlock(rect, HIGHLIGHT_TINT);
						}

						// Draw GUI
						var (preview, palLabel) = GetJujubePaletteCache(palette[index]);
						if (preview != null) {
							GUI.DrawTexture(rect, preview);
							if (!string.IsNullOrEmpty(palLabel)) {
								var labelRect = new Rect(
									rect.x + itemSize * 0.5f,
									rect.y + itemSize * 0.5f,
									itemSize * 0.5f,
									itemSize * 0.5f
								);
								ColorBlock(
									labelRect,
									EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 0.7f) : new Color(0.6f, 0.6f, 0.6f, 0.7f)
								);
								GUI.Label(labelRect, palLabel, CenterLabelStyle_10);
							}
						} else {
							GUI.Box(rect, GUIContent.none);
						}

						// Mouse Event
						if (mouseDown && downIndex < 0 && rect.Contains(Event.current.mousePosition)) {
							downIndex = index;
						}

						// Final
						index++;
					}
				});
			}
			Space(36);

			// Mouse Down
			if (downIndex >= 0) {
				if (Event.current.button == 0) {
					// Left
					SelectingPaletteItemIndex = downIndex;
				} else if (Event.current.button == 1) {
					// Right
					ShowPaletteItemMenu(downIndex);
				}
				Event.current.Use();
			}

			// End
			EditorGUILayout.EndScrollView();

		}


		private static void SceneGUI_Toolbar (SceneView sceneView) {
			var rect = new Rect(
				12 + PANEL_PADDING + PANEL_WIDTH + PANEL_PADDING,
				6,
				TOOLBAR_WIDTH + PANEL_PADDING + PANEL_PADDING + PANEL_WIDTH,
				sceneView.position.height
			);
			bool oldE = GUI.enabled;
			var layer = EditingRenderer.Map[SelectingLayerIndex];
			GUI.enabled = layer != null && layer.Visible;
			var panelRect = new Rect(0, 0, TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M * ToolCount);
			HandlesLayoutV(rect, () => {

				// Tools 
				for (int i = 0; i < ToolCount; i++) {

					var tool = (JujubeTool)i;
					var toolRect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
					if (GUI.Button(toolRect, GUIContent.none)) {
						UsingTool = tool;
					}

					if (UsingTool == tool) {
						// Highlight
						ColorBlock(new Rect(
							toolRect.x + 1,
							toolRect.y + 1,
							toolRect.width - 2,
							toolRect.height - 2
						), EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.25f) : new Color(0f, 0f, 0f, 0.25f));

						var labelRect = toolRect;
						labelRect.x += toolRect.width + 4;
						labelRect.width = PANEL_WIDTH;

						// Label
						GUI.Label(labelRect, tool.ToString());

					}

					// Icon
					var icon = GetJujubeImage(TOOL_ICONS[i + (EditorGUIUtility.isProSkin ? 0 : TOOL_ICONS.Length / 2)]);
					if (icon != null) {
						GUI.DrawTexture(new Rect(
							toolRect.x + 9,
							toolRect.y + 9,
							toolRect.width - 12,
							toolRect.height - 12
						), icon, ScaleMode.StretchToFill);
					}

					// Hotkey Number Label
					GUI.Label(new Rect(
						toolRect.x,
						toolRect.y,
						toolRect.width * 0.5f,
						toolRect.height * 0.5f
					), ((int)tool + 1).ToString(), EditorStyles.miniLabel);

				}

				Space(6);

				// Tool Option
				Rect _rect = panelRect;
				switch (UsingTool) {

					case JujubeTool.Select:
					case JujubeTool.Wand:
					case JujubeTool.Erase: {

						// Layer Mode
						_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
						string _label = LAYER_MODE_LABELS[LayerModeIndex.Value];
						if (GUI.Button(_rect, _label[0].ToString())) {
							LayerModeIndex.Value = Mathf.RoundToInt(Mathf.Repeat(
								LayerModeIndex.Value + 1f,
								LAYER_MODE_LABELS.Length
							));
						}
						_rect.x += _rect.width + 6;
						_rect.width = PANEL_WIDTH - _rect.x;
						GUI.Label(_rect, _label);


						if (UsingTool == JujubeTool.Select || UsingTool == JujubeTool.Wand) {

							// Wand Option
							if (UsingTool == JujubeTool.Wand) {

								// Wand Option
								_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
								_label = WAND_OPTION_LABELS[WandModeIndex.Value];
								if (GUI.Button(_rect, _label[0].ToString())) {
									WandModeIndex.Value = Mathf.RoundToInt(Mathf.Repeat(
										WandModeIndex.Value + 1f,
										WAND_OPTION_LABELS.Length
									));
									Event.current.Use();
								}
								_rect.x += _rect.width + 6;
								_rect.width = PANEL_WIDTH - _rect.x;
								GUI.Label(_rect, _label);

								// Wand Spread
								_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
								_label = WAND_SPREAD_LABELS[WandSpreadIndex.Value];
								if (GUI.Button(_rect, _label[0].ToString())) {
									WandSpreadIndex.Value = Mathf.RoundToInt(Mathf.Repeat(
										WandSpreadIndex.Value + 1f,
										WAND_SPREAD_LABELS.Length
									));
									Event.current.Use();
								}
								_rect.x += _rect.width + 6;
								_rect.width = PANEL_WIDTH - _rect.x;
								GUI.Label(_rect, _label);
							}

							// Rotate Selection
							bool oldEAlt = GUI.enabled;
							GUI.enabled = GUI.enabled && SelectingBlockMap.Count > 0;
							_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
							if (GUI.Button(_rect, new GUIContent(GetJujubeImage($"RotateCW{(EditorGUIUtility.isProSkin ? "_pro" : "_per")}.png")))) {
								RotateSelectingBlocks(true);
								RefreshMapBound();
								Event.current.Use();
							}
							_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
							if (GUI.Button(_rect, new GUIContent(GetJujubeImage($"RotateACW{(EditorGUIUtility.isProSkin ? "_pro" : "_per")}.png")))) {
								RotateSelectingBlocks(false);
								RefreshMapBound();
								Event.current.Use();
							}
							GUI.enabled = oldEAlt;

						}

						break;
					}
					case JujubeTool.Brush: {
						// Brush Option
						bool oldEAlt = GUI.enabled;
						GUI.enabled = GUI.enabled && AllowRotatePaintingBlock;
						_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
						var _label = CURSOR_ROTATION_LABELS[CursorRotationIndex.Value];
						if (GUI.Button(_rect, AllowRotatePaintingBlock ? _label : "-")) {
							CursorRotationIndex.Value = Mathf.RoundToInt(Mathf.Repeat(
								CursorRotationIndex.Value + 1f,
								CURSOR_ROTATION_LABELS.Length / 2
							));
							ClearCursorPrefab();
							Event.current.Use();
						}
						_rect.x += _rect.width + 6;
						_rect.width = PANEL_WIDTH - _rect.x;
						_label = CURSOR_ROTATION_LABELS[CursorRotationIndex.Value + CURSOR_ROTATION_LABELS.Length / 2];
						GUI.Label(_rect, _label);
						GUI.enabled = oldEAlt;
						break;
					}

					case JujubeTool.Paint: {

						// Paint Option
						_rect = GUIRect(TOOLBAR_WIDTH, TOOLBAR_BUTTON_HEIGHT_M);
						var _label = new GUIContent(GetJujubeImage($"Paint_{PaintMode}_{(EditorGUIUtility.isProSkin ? "pro" : "per")}.png"));
						if (GUI.Button(_rect, _label)) {
							PaintModeIndex.Value = Mathf.RoundToInt(Mathf.Repeat(
								PaintModeIndex.Value + 1f,
								PAINT_OPTION_LABELS.Length
							));
							Event.current.Use();
						}
						_rect.x += _rect.width + 6;
						_rect.width = PANEL_WIDTH - _rect.x;
						GUI.Label(_rect, PAINT_OPTION_LABELS[PaintModeIndex.Value]);

						break;
					}
				}

				// MouseInPanelGUI
				if (!MouseInPanelGUI) {
					panelRect.height = _rect.y + _rect.height;
					MouseInPanelGUI = MouseInPanelGUI || panelRect.Contains(Event.current.mousePosition);
				}
			});
			GUI.enabled = oldE;

		}


		private static void SceneGUI_State (SceneView sceneView) {
			if (!ShowStateLabel.Value || EditingRenderer == null || EditingRenderer.Map == null) { return; }
			if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) { return; }
			var rect = new Rect(
				6,
				36 + PANEL_TITLE_HEIGHT + LAYER_CONTENT_HEIGHT + PANEL_TITLE_HEIGHT + PALETTE_CONTENT_HEIGHT + PANEL_TITLE_HEIGHT,
				PANEL_WIDTH,
				sceneView.position.height
			);
			rect.height -= rect.y;
			HandlesLayoutV(rect, () => {

				LayoutH(() => {
					if (BlockDirtyCount < 0) {
						BlockDirtyCount = EditingRenderer.Map.GetBlockCount();
					}
					GUI.Label(GUIRect(0, PANEL_ITEM_HEIGHT), "Block Count");
					GUI.Label(GUIRect(0, PANEL_ITEM_HEIGHT), BlockDirtyCount.ToString());
				});
				LayoutH(() => {
					GUI.Label(GUIRect(0, PANEL_ITEM_HEIGHT), "Selection");
					string label = "-";
					if (SelectingBlockMap.Count > 0) {
						label = $"{SelectingBlockMax.x - SelectingBlockMin.x + 1} × {SelectingBlockMax.y - SelectingBlockMin.y + 1} × {SelectingBlockMax.z - SelectingBlockMin.z + 1}";
					} else if (Paint_CursorDownMapPosition.HasValue && CursorPos.HasValue) {
						var cursorPos = GetBlockLocalPosition(CursorPos.Value);
						float cellSize = EditingRenderer.CellSize;
						var min = Vector3.Min(Paint_CursorDownMapPosition.Value, cursorPos);
						var max = Vector3.Max(Paint_CursorDownMapPosition.Value, cursorPos);
						min.y = Mathf.Max(min.y, 0);
						max.y = Mathf.Max(max.y, 0);
						label = $"{Mathf.RoundToInt(max.x - min.x + 1)} × {Mathf.RoundToInt(max.y - min.y + 1)} × {Mathf.RoundToInt(max.z - min.z + 1)}";
					}
					GUI.Label(GUIRect(0, PANEL_ITEM_HEIGHT), label);
				});
				LayoutH(() => {
					GUI.Label(GUIRect(0, PANEL_ITEM_HEIGHT), "Refresh");
					var oldC = GUI.color;
					GUI.color = new Color(1, 1, 1, 0.2f);
					GUI.Label(GUIRect(0, PANEL_ITEM_HEIGHT), SCENE_REFRESH_LABELS[(int)Mathf.PingPong(
						SceneRefreshingLabelAmount,
						SCENE_REFRESH_LABELS.Length - 0.01f
					)]);
					GUI.color = oldC;
					SceneRefreshingLabelAmount += 0.2f;
				});
				Space(4);

				// Thumbnail
				if (UsingTool == JujubeTool.Brush || UsingTool == JujubeTool.Paint) {
					LayoutH(() => {
						Space(4);
						var thumbnailRect = GUIRect(THUMBNAIL_SIZE, THUMBNAIL_SIZE);
						var prefab = EditingRenderer.Palette != null && SelectingPaletteItemIndex >= 0 && SelectingPaletteItemIndex < EditingRenderer.Palette.Count ? EditingRenderer.Palette[SelectingPaletteItemIndex] : null;
						if (EditorGUIUtility.isProSkin) {
							GUI.Box(thumbnailRect, GUIContent.none);
						} else {
							ColorBlock(thumbnailRect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
						}
						if (prefab) {
							var (thumbnail, _) = GetJujubePaletteCache(prefab);
							if (thumbnail != null) {
								GUI.DrawTexture(thumbnailRect, thumbnail);
								string fullLabel = prefab.name;
								if (!string.IsNullOrEmpty(fullLabel)) {
									if (fullLabel.Length >= 10) {
										fullLabel = fullLabel.Substring(0, 7) + "...";
									}
									var labelRect = new Rect(
										thumbnailRect.x,
										thumbnailRect.y + THUMBNAIL_SIZE * 0.7f,
										THUMBNAIL_SIZE,
										THUMBNAIL_SIZE * 0.25f
									);
									ColorBlock(
										labelRect,
										EditorGUIUtility.isProSkin ? new Color(0, 0, 0, 0.7f) : new Color(0.6f, 0.6f, 0.6f, 0.7f)
									);
									GUI.Label(labelRect, fullLabel, CenterLabelStyle_10);
								}
							}
						}
					});
				}

			});
		}


		private static void SceneGUI_Warning (SceneView sceneView) {

			if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout) { return; }

			string message = string.Empty;
			float height = 0f;

			// Layer Not Visible
			var layer = EditingRenderer.Map[SelectingLayerIndex];
			if (layer != null && !layer.Visible) {
				message += "Selecting Layer is Not Visible";
				height += 22f;
			}
			bool hasMessage = !string.IsNullOrEmpty(message);
			bool hasTempWarning = EditorApplication.timeSinceStartup < TempWarningTime + TEMP_WARNING_DURATION;

			if (!hasMessage && !hasTempWarning) { return; }

			var rect = new Rect(
				36 + TOOLBAR_WIDTH + PANEL_WIDTH * 2 + PANEL_PADDING * 2,
				36, 280, height + (hasTempWarning ? 24 : 0f)
			);
			HandlesLayoutV(rect, () => {
				if (hasMessage) {
					var labelRect = GUIRect(0, height);
					ColorBlock(labelRect, new Color(1f, 0.4f, 0f, 0.5f));
					GUI.Label(labelRect, message, new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleCenter,
					});
				}
				if (hasTempWarning) {
					Space(2);
					var labelRect = GUIRect(0, 22);
					ColorBlock(labelRect, new Color(1f, 0.4f, 0f, 0.5f * Mathf.Clamp01(Mathf.LerpUnclamped(0f, 5f, TempWarningTime + TEMP_WARNING_DURATION - (float)EditorApplication.timeSinceStartup))));
					GUI.Label(labelRect, TempWarningMessage, new GUIStyle(GUI.skin.label) {
						alignment = TextAnchor.MiddleCenter,
					});
					sceneView.Repaint();
				}
			});
		}


		private static void SceneGUI_Panel_Drag (Rect panelRect) {
			// Drag to Add Prefab into Palette
			switch (Event.current.type) {
				case EventType.DragUpdated:
					if (!panelRect.Contains(Event.current.mousePosition)) { break; }
					foreach (var obj in DragAndDrop.objectReferences) {
						if (obj != null && obj is GameObject gObj && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(gObj))) {
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
							break;
						}
					}
					break;
				case EventType.DragPerform:
					if (!panelRect.Contains(Event.current.mousePosition)) { break; }
					bool undoRegisted = false;
					foreach (var obj in DragAndDrop.objectReferences) {
						if (obj != null && obj is GameObject gObj && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(gObj))) {
							if (!undoRegisted) {
								RegisterJujubeUndo();
								SetPaletteAssetDirty();
								undoRegisted = true;
							}
							EditingRenderer.Palette.AddItem(gObj);
						}
					}
					if (undoRegisted) {
						SceneView.RepaintAll();
					}
					DragAndDrop.AcceptDrag();
					Event.current.Use();
					break;
			}
		}


		#endregion




		#region --- LGC ---


		// Popup Menu
		private static void ShowMapSelectorPopupMenu () {
			var menu = new GenericMenu() { allowDuplicateNames = false, };
			var renderers = new List<JujubeRenderer>(Object.FindObjectsOfType<JujubeRenderer>());
			renderers.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
			for (int i = 0; i < renderers.Count; i++) {
				var renderer = renderers[i];
				if (renderer == null) { continue; }
				if (renderer.Map != null && renderer.Mode == JujubeRendererMode.Develop) {
					menu.AddItem(
						new GUIContent(renderer.name),
						renderer == EditingRenderer,
						() => StartEdit(renderer)
					);
				} else {
					menu.AddDisabledItem(new GUIContent(renderer.name));
				}
			}
			menu.ShowAsContext();
		}


		private static void ShowLayerItemMenu (int layerItemIndex) {

			if (EditingRenderer == null || EditingRenderer.Map == null) { return; }

			var menu = new GenericMenu() { allowDuplicateNames = false, };
			menu.AddDisabledItem(new GUIContent(EditingRenderer.Map[layerItemIndex].LayerName));
			menu.AddSeparator("");

			// Move Up
			if (layerItemIndex > 0) {
				menu.AddItem(
					new GUIContent("Move Up"),
					false,
					() => MoveLayerUp(layerItemIndex)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Move Up"));
			}


			// Move Down
			if (layerItemIndex < EditingRenderer.Map.LayerCount - 1) {
				menu.AddItem(
					new GUIContent("Move Down"),
					false,
					() => MoveLayerDown(layerItemIndex)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Move Down"));
			}

			// Delete
			menu.AddItem(
				new GUIContent("Delete"),
				false,
				() => DeleteLayer(layerItemIndex)
			);

			menu.AddSeparator("");

			// Merge Down
			if (layerItemIndex < EditingRenderer.Map.LayerCount - 1) {
				menu.AddItem(
					new GUIContent("Merge Down"),
					false,
					() => MergeLayerDown(layerItemIndex)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Merge Down"));
			}

			// Remove Overlapped Blocks
			menu.AddItem(
				new GUIContent("Remove Overlapped Blocks"),
				false,
				() => RemoveOverlappedBlocks(layerItemIndex)
			);


			// Show
			menu.ShowAsContext();
		}


		private static void ShowPaletteItemMenu (int paletteItemIndex) {
			if (EditingRenderer == null || EditingRenderer.Palette == null) { return; }
			var menu = new GenericMenu() { allowDuplicateNames = false, };
			var prefab = EditingRenderer.Palette[paletteItemIndex];
			menu.AddDisabledItem(new GUIContent(
				prefab != null ? prefab.name : "-"
			));
			menu.AddSeparator("");
			menu.AddItem(
				new GUIContent("Duplicate"),
				false,
				() => DuplicatePaletteItem(paletteItemIndex)
			);
			menu.AddItem(
				new GUIContent("Replace"),
				false,
				() => PickPalettePrefab(paletteItemIndex)
			);
			menu.AddItem(
				new GUIContent("Delete"),
				false,
				() => DeletePaletteItem(paletteItemIndex)
			);
			menu.AddSeparator("");

			if (paletteItemIndex > 0) {
				menu.AddItem(
					new GUIContent("Move Left"),
					false,
					() => SwipePaletteItem(paletteItemIndex, paletteItemIndex - 1)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Move Left"));
			}

			if (paletteItemIndex < EditingRenderer.Palette.Count - 1) {
				menu.AddItem(
					new GUIContent("Move Right"),
					false,
					() => SwipePaletteItem(paletteItemIndex, paletteItemIndex + 1)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Move Right"));
			}

			if (paletteItemIndex > 0) {
				menu.AddItem(
					new GUIContent("Move to First"),
					false,
					() => MovePaletteItem(paletteItemIndex, 0)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Move to First"));
			}

			if (paletteItemIndex < EditingRenderer.Palette.Count - 1) {
				menu.AddItem(
					new GUIContent("Move to Last"),
					false,
					() => MovePaletteItem(paletteItemIndex, EditingRenderer.Palette.Count - 1)
				);
			} else {
				menu.AddDisabledItem(new GUIContent("Move to Last"));
			}
			menu.AddSeparator("");
			menu.AddItem(
				new GUIContent("Sort by Name"),
				false,
				SortPaletteItemByName
			);

			menu.ShowAsContext();
		}


		// Palette
		private static void PickPalettePrefab (int index = -1) {
			string path = EditorUtility.OpenFilePanel("Pick Prefab", "Assets", "prefab");
			if (EditorUtil.FileExists(path) && EditingRenderer != null && EditingRenderer.Palette != null) {
				var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorUtil.FixedRelativePath(path));
				if (prefab != null) {
					SetMapDirty(true);
					SetPaletteAssetDirty();
					RegisterJujubeUndo();
					if (index < 0 || index >= EditingRenderer.Palette.Count) {
						EditingRenderer.Palette.AddItem(prefab);
						SelectingPaletteItemIndex = EditingRenderer.Palette.Count - 1;
						ScrollPaletteToSelection();
					} else {
						EditingRenderer.Palette.SetItemPrefab(index, prefab);
						SelectingPaletteItemIndex = index;
					}
				}
			}
		}


		private static void DeletePaletteItem (int index) {
			if (EditingRenderer == null || EditingRenderer.Palette == null) { return; }
			var prefab = index >= 0 && index < EditingRenderer.Palette.Count ? EditingRenderer.Palette[index] : null;
			bool delete = EditorUtil.Dialog("Delete", $"Delete palette item {(prefab != null ? prefab.name : "-")}?", "Delete", "Cancel");
			if (delete) {
				SetMapDirty(true);
				SetPaletteAssetDirty();
				RegisterJujubeUndo();
				EditingRenderer.Palette.DeleteItem(index, EditingRenderer.Map);
			}
		}


		private static void DuplicatePaletteItem (int index) {
			if (EditingRenderer == null || EditingRenderer.Palette == null) { return; }
			SetMapDirty(true);
			SetPaletteAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Palette.DuplicateItem(index, EditingRenderer.Map);
		}


		private static void SwipePaletteItem (int indexA, int indexB) {
			if (EditingRenderer == null || EditingRenderer.Palette == null) { return; }
			SetMapDirty(true);
			SetPaletteAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Palette.SwipeItem(indexA, indexB, EditingRenderer.Map);
		}


		private static void MovePaletteItem (int index, int newIndex) {
			if (EditingRenderer == null || EditingRenderer.Palette == null) { return; }
			SetMapDirty(true);
			SetPaletteAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Palette.MoveItem(index, newIndex, EditingRenderer.Map);
		}


		private static void ScrollPaletteToSelection () {
			if (EditingRenderer == null || EditingRenderer.Palette == null) { return; }
			int countX = PaletteItemCountX;
			PalettePanelScrollPosition.y = Mathf.Max(0, PANEL_WIDTH / countX * (
				(SelectingPaletteItemIndex / countX) - 2
			));
		}


		private static void SortPaletteItemByName () {
			if (EditingRenderer != null && EditingRenderer.Palette != null && EditingRenderer.Palette.Count == 0) { return; }
			SetMapDirty(true);
			SetPaletteAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Palette.SortByName(EditingRenderer.Map);
		}


		// Layer
		private static void DeleteLayer (int index) {
			if (EditingRenderer == null || EditingRenderer.Map == null || index < 0 || index >= EditingRenderer.Map.LayerCount) { return; }
			var layer = EditingRenderer.Map[index];
			if (EditingRenderer.Map.LayerCount <= 1) {
				EditorUtil.Dialog("Delete", "Can not delete last layer.", "OK");
				return;
			}
			bool delete = EditorUtil.Dialog("Delete", $"Delete layer {layer.LayerName}?", "Delete", "Cancel");
			if (delete) {
				SetMapDirty(true);
				SetMapAssetDirty();
				//SetJblockDirty();
				RegisterJujubeUndo();
				EditingRenderer.Map.RemoveLayer(index);
			}
		}


		private static void MoveLayerUp (int index) {
			SetMapDirty(true);
			SetMapAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Map.SwipeLayer(index, index - 1);
			if (SelectingLayerIndex == index) {
				SelectingLayerIndex--;
			} else if (index - 1 == SelectingLayerIndex) {
				SelectingLayerIndex++;
			}
		}


		private static void MoveLayerDown (int index) {
			SetMapDirty(true);
			SetMapAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Map.SwipeLayer(index, index + 1);
			if (SelectingLayerIndex == index) {
				SelectingLayerIndex++;
			} else if (index + 1 == SelectingLayerIndex) {
				SelectingLayerIndex--;
			}
		}


		private static void MergeLayerDown (int index) {
			SetMapDirty(true);
			SetMapAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Map.MergeLayer(index, index + 1);
		}


		private static void RemoveOverlappedBlocks (int index) {
			SetMapDirty(true);
			SetMapAssetDirty();
			RegisterJujubeUndo();
			EditingRenderer.Map.RemoveOverlappedBlocks(index);
		}


		// Message
		private static void LogTempWarningMessage (string message) {
			TempWarningMessage = message;
			TempWarningTime = (float)EditorApplication.timeSinceStartup;
		}


		#endregion




	}
}