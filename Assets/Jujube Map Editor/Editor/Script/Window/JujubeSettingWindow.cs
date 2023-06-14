namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;



	public class JujubeSettingWindow : MoenenEditorWindow {


		// Const
		private const float FIELD_WIDTH = 64;
		private const float FIELD_HEIGHT = 18;

		// Short
		private static GUIStyle PaddingLayoutStyle => _PaddingLayoutStyle != null ? _PaddingLayoutStyle : (_PaddingLayoutStyle = new GUIStyle(GUI.skin.box) {
			margin = new RectOffset(24, 64, 6, 6),
			padding = new RectOffset(24, 4, 4, 4),
		});

		// Data
		private static GUIStyle _PaddingLayoutStyle = null;
		private static Vector2 ScrollPosition = default;

		// Saving
		private static EditorSavingBool Opening_General = new EditorSavingBool("JujubeSettingWindow.Opening_General", true);
		private static EditorSavingBool Opening_Hotkeys = new EditorSavingBool("JujubeSettingWindow.Opening_Hotkeys", false);


		// MSG
		private void OnGUI () {

			Space(12);

			ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition);

			// General
			bool opening = Opening_General.Value;
			LayoutF(() => {

				Space(2);

				// Show State
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Show State");
					bool newShowState = EditorGUI.Toggle(
						GUIRect(FIELD_HEIGHT, FIELD_HEIGHT), GUIContent.none, JujubeScene.ShowStateLabel.Value
					);
					if (newShowState != JujubeScene.ShowStateLabel.Value) {
						JujubeScene.ShowStateLabel.Value = newShowState;
					}
				});
				Space(2);


				// Use Block Shake UseBlockShake
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Shake Selecting Block");
					bool newBlockShake = EditorGUI.Toggle(
						GUIRect(FIELD_HEIGHT, FIELD_HEIGHT), GUIContent.none, JujubeScene.UseBlockShake.Value
					);
					if (newBlockShake != JujubeScene.UseBlockShake.Value) {
						JujubeScene.UseBlockShake.Value = newBlockShake;
						JujubeScene.ClearBlockSelection();
						JujubeScene.SetMapDirty(false);
					}
				});
				Space(2);

				// Show Editing Hierarchy Label
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Hierarchy Editing Label");
					bool newShowLabel = EditorGUI.Toggle(
						GUIRect(FIELD_HEIGHT, FIELD_HEIGHT), GUIContent.none, JujubeScene.ShowEditingHierarchyLabel.Value
					);
					if (newShowLabel != JujubeScene.ShowEditingHierarchyLabel.Value) {
						JujubeScene.ShowEditingHierarchyLabel.Value = newShowLabel;
						EditorApplication.RepaintHierarchyWindow();
					}
				});
				Space(2);


				// Palette Item Size 
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Palette Item Size");
					JujubeScene.PaletteItemSizeIndex.Value = (int)(JujubeScene.PaletteItemSizeMode)EditorGUI.EnumPopup(
						GUIRect(FIELD_WIDTH, FIELD_HEIGHT), (JujubeScene.PaletteItemSizeMode)JujubeScene.PaletteItemSizeIndex.Value
					);
				});
				Space(2);

				//Position Handle Type
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Position Handle Style");
					JujubeScene.PositionHandleTypeIndex.Value = (int)(JujubeScene.JujubePositionHandleType)EditorGUI.EnumPopup(
						GUIRect(FIELD_WIDTH, FIELD_HEIGHT), (JujubeScene.JujubePositionHandleType)JujubeScene.PositionHandleTypeIndex.Value
					);
				});
				Space(2);

				// Max Selection Count
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Max Selection Count");
					JujubeScene.MaxSelectionCount.Value = Mathf.Clamp(EditorGUI.DelayedIntField(
						GUIRect(FIELD_WIDTH, FIELD_HEIGHT),
						JujubeScene.MaxSelectionCount.Value
					), 64, 16384);
				});
				Space(2);

				// Max Paint Count
				LayoutH(() => {
					GUI.Label(GUIRect(0, FIELD_HEIGHT), "Max Paint Count");
					JujubeScene.MaxPaintCount.Value = Mathf.Clamp(EditorGUI.DelayedIntField(
						GUIRect(FIELD_WIDTH, FIELD_HEIGHT),
						JujubeScene.MaxPaintCount.Value
					), 64, 16384);
				});
				Space(2);

				// Change
				if (GUI.changed) {
					SceneView.RepaintAll();
				}

			}, "General", ref opening, false, PaddingLayoutStyle);
			Opening_General.Value = opening;



			// Hotkey
			opening = Opening_Hotkeys.Value;
			LayoutF(() => {
				Space(4);

				var richLabelStyle = new GUIStyle(GUI.skin.label) {
					richText = true,
				};

				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[M] or [1]</color> Select Tool", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[N] or [2]</color> Wand Tool", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[B] or [3]</color> Brush Tool", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[R] or [4]</color> Erase Tool", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[G] or [5]</color> Paint Tool", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[P] or [6]</color> Pick Tool", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Q] or [E]</color> Rotate Cursor/Selection", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Z] or [X]</color> Select Next/ Prev Layer", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[ESC]</color> Deselect Blocks", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[F5]</color> Reload Map", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Shift+C]</color> Show/Hide Current Layer", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Shift+WASD]</color> Move Palette Cursor", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[WASD][↑↓←→][-=]</color> Move Selecting Blocks", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Shift+LeftClick]</color> Pick Prefab", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Shift+RightClick]</color> Move Camera", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Ctrl+LeftClick]</color> Append Selection", richLabelStyle);
				GUI.Label(GUIRect(0, FIELD_HEIGHT), "<color=#FFCC66>[Ctrl+Shift+LeftClick]</color> Remove Selection", richLabelStyle);

				Space(4);
			}, "Hotkey Manual", ref opening, false, PaddingLayoutStyle);
			Opening_Hotkeys.Value = opening;


			GUIRect(1, 0);

			EditorGUILayout.EndScrollView();

			Space(4);

			// Reset
			LayoutH(() => {
				GUIRect(0, FIELD_HEIGHT);
				if (GUI.Button(GUIRect(92, FIELD_HEIGHT), "Reset All")) {
					EditorApplication.delayCall += ShowResetDialog;
				}
				Space(6);
			});
			Space(6);

			if (Event.current.type == EventType.MouseDown) {
				GUI.FocusControl("");
				Repaint();
			}

		}


		// LGC
		private void ShowResetDialog () {
			bool reset = EditorUtil.Dialog("Reset Jujube Setting", "Reset All Settings for Jujube Map Editor ?", "Reset ?", "Cancel");
			if (reset) {
				reset = EditorUtil.Dialog("Reset Jujube Setting", "Reset All Settings for Jujube Map Editor ?", "Reset !", "Cancel");
				if (reset) {
					JujubeScene.PaletteItemSizeIndex.Reset();
					JujubeScene.CursorRotationIndex.Reset();
					JujubeScene.SelectingToolIndex.Reset();
					JujubeScene.LayerModeIndex.Reset();
					JujubeScene.PositionHandleTypeIndex.Reset();
					JujubeScene.ShowStateLabel.Reset();
					JujubeScene.UseBlockShake.Reset();
					JujubeScene.MaxSelectionCount.Reset();
					JujubeScene.MaxPaintCount.Reset();
					SceneView.RepaintAll();
				}
			}
		}


	}
}