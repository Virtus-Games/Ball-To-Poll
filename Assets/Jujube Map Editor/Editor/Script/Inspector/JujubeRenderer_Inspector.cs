namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	
	using JujubeMapEditor.Core;




	[CustomEditor(typeof(JujubeRenderer), true)]
	public class JujubeRenderer_Inspector : MoenenInspector {



		// Short
		private static Texture2D JujubIcon => _JujubIcon != null ? _JujubIcon : (_JujubIcon = EditorUtil.GetImage(EditorGUIUtility.isProSkin ? "Icon.png" : "Icon Dark.png"));
		private static Texture2D _JujubIcon = null;

		// Data
		private JujubeMap PrevMap = null;
		private JujubePalette PrevPalette = null;
		private JujubePrefabScaleMode PrevPrefabScaleMode = default;
		private JujubePrefabPivotMode PrevPrefabPivot = default;
		private float PrevCellSize = 1f;
		private SerializedProperty[] Props_A = null;
		private SerializedProperty[] Props_B = null;


		// MSG
		private void OnEnable () {
			var jTarget = target as JujubeRenderer;
			PrevMap = jTarget.Map;
			PrevPalette = jTarget.Palette;
			PrevPrefabScaleMode = jTarget.PrefabScale;
			PrevPrefabPivot = jTarget.PrefabPivot;
			PrevCellSize = jTarget.CellSize;
			Props_A = new SerializedProperty[]{
				serializedObject.FindProperty("m_Map"),
				serializedObject.FindProperty("m_Palette"),
			};
			Props_B = new SerializedProperty[]{
				serializedObject.FindProperty("m_CellSize"),
				serializedObject.FindProperty("m_PrefabScale"),
				serializedObject.FindProperty("m_PrefabPivot"),
				serializedObject.FindProperty("m_Mode"),
			};
		}


		public override void OnInspectorGUI () {
			GUI_Properties();
			GUI_ControlButtons();
			GUI_HelpBox();
		}


		private void GUI_Properties () {

			var jTarget = target as JujubeRenderer;
			bool missAsset = jTarget.Map == null || jTarget.Palette == null;
			serializedObject.Update();

			foreach (var prop in Props_A) {
				EditorGUILayout.PropertyField(prop);
			}
			if (missAsset) {
				Space(4);
				if (jTarget.Map == null) {
					if (GUI.Button(GUIRect(0, 24), "Create Map Asset")) {
						var map = CreateAsset<JujubeMap>(
							"New Jujube Map",
							"[Jujube Map Editor] Fail to create map asset."
						);
						if (map != null) {
							map.AddLayer();
							if (map.LayerCount > 0) {
								map[0].Blocks.Add(new JujubeBlock() {
									X = 0,
									Y = 0,
									Z = 0,
									Index = 0,
									RotZ = 0,
								});
							}
							jTarget.SetMap(map);
						}
						EditorUtility.SetDirty(jTarget);
					}
				}
				Space(4);
				if (jTarget.Palette == null) {
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
				}
				Space(4);
			} else {
				foreach (var prop in Props_B) {
					EditorGUILayout.PropertyField(prop);
				}
			}


			serializedObject.ApplyModifiedProperties();
		}


		private void GUI_ControlButtons () {
			const int ITEM_HEIGHT = 24;
			var jTarget = target as JujubeRenderer;
			// Reload On Change
			if (
				jTarget.Map != PrevMap ||
				jTarget.Palette != PrevPalette ||
				jTarget.PrefabScale != PrevPrefabScaleMode ||
				jTarget.PrefabPivot != PrevPrefabPivot ||
				jTarget.CellSize != PrevCellSize
			) {
				if (JujubeScene.EditingRenderer == jTarget) {
					JujubeScene.RefreshMapBound();
				}
				jTarget.ReloadAll(jTarget.Map != PrevMap || jTarget.Palette != PrevPalette);
				PrevMap = jTarget.Map;
				PrevPalette = jTarget.Palette;
				PrevPrefabScaleMode = jTarget.PrefabScale;
				PrevPrefabPivot = jTarget.PrefabPivot;
				PrevCellSize = jTarget.CellSize;
			}
			if (jTarget.Map != null && jTarget.Palette != null) {
				// Buttons
				Space(6);
				LayoutH(() => {
					bool oldE = GUI.enabled;
					GUI.enabled = jTarget.Editable;
					if (UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() == null && !PrefabUtility.IsPartOfPrefabAsset(jTarget)) {
						if (!EditorApplication.isPlaying) {
							// Edit
							bool editing = JujubeScene.EditingRenderer == jTarget;
							if (GUI.Button(GUIRect(0, ITEM_HEIGHT), new GUIContent(editing ? " Done  " : " Edit  ", JujubIcon))) {
								if (editing) {
									JujubeScene.EndEdit();
								} else {
									EditorApplication.delayCall += () => {
										bool confirm = true;
										if (jTarget.Mode != JujubeRendererMode.Develop) {
											confirm = EditorUtil.Dialog("Confirm", "Change this map to \"Develop Mode\" and edit?", "Change and Edit", "Cancel");
											if (confirm) {
												jTarget.Mode = JujubeRendererMode.Develop;
												jTarget.ReloadAll(true);
											}
										}
										if (confirm) {
											JujubeScene.StartEdit(jTarget);
										}
									};
								}
							}
						}

						// Reload
						var nRoot = PrefabUtility.GetNearestPrefabInstanceRoot(jTarget.gameObject);
						GUI.enabled = nRoot == null;
						if (GUI.Button(GUIRect(0, ITEM_HEIGHT), "Reload")) {
							jTarget.ReloadAll(true);
							EditorUtility.SetDirty(target);
						}
					}
					GUI.enabled = oldE;
				});

				// Export
				LayoutH(() => {
					// Export Mesh
					if (GUI.Button(GUIRect(0, ITEM_HEIGHT), new GUIContent(" Export Combined Mesh", GetJujubeImage("Mesh Icon.png")))) {
						var window = JujubeCombinerWindow.OpenCombinerWindow();
						window.Map = jTarget.Map;
						window.Palette = jTarget.Palette;
						window.titleContent = new GUIContent("Export Mesh");
						window.PrefabPivot = jTarget.PrefabPivot;
						window.PrefabScale = jTarget.PrefabScale;
						window.CellSize = jTarget.CellSize;
					}
				});
			}
		}


		private void GUI_HelpBox () {
			var jTarget = target as JujubeRenderer;
			if (!jTarget.Editable) {
				Space(4);
				var msgType = MessageType.Info;
				int boxHeight = 32;
				string helpMSG = "Can not edit this map because:\n";
				if (jTarget.Map == null) {
					helpMSG += "  - No map asset linked to this map.\n";
					msgType = MessageType.Warning;
					boxHeight += 16;
				}
				if (jTarget.Palette == null) {
					helpMSG += "  - No palette asset linked to this map.\n";
					msgType = MessageType.Warning;
					boxHeight += 16;
				}
				EditorGUI.HelpBox(GUIRect(0, boxHeight), helpMSG, msgType);
				Space(4);
			}
		}


	}
}