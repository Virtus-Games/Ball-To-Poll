namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using JujubeMapEditor.Core;


	[AddComponentMenu("Jujube/Adaptive Block")]
	public class AdaptiveBlock : JBlock {




		#region --- SUB ---


		public enum AdaptiveState {
			Anything = 0,
			Solid = 1,
			Air = 2,
		}


		public enum AdaptivePosition {
			Down = 0,
			Up = 1,
			Back = 2,
			Forward = 3,
			Left = 4,
			Right = 5,
		}



		[System.Serializable]
		public class AdaptiveData {

			public GameObject Prefab = null;
			public short Adaptive = 0;


			public AdaptiveState GetAdaptiveState (AdaptivePosition pos) {
				return
					!GetBit(Adaptive, (int)pos + 6) ? AdaptiveState.Anything :
					GetBit(Adaptive, (int)pos) ? AdaptiveState.Solid :
					AdaptiveState.Air;
			}


			public void SetAdaptiveState (AdaptivePosition pos, AdaptiveState state) {
				int index = (int)pos;
				Adaptive = SetBitValue(Adaptive, index + 6, state != AdaptiveState.Anything);
				if (state != AdaptiveState.Anything) {
					Adaptive = SetBitValue(Adaptive, index, state == AdaptiveState.Solid);
				}
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly Vector3Int[] ADP_POS_XYZ = { new Vector3Int(0, -1, 0), new Vector3Int(0, 1, 0), new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0), };

		// Api
		public List<AdaptiveData> Prefabs => m_Prefabs;
		public bool AdaptDifferentBlock { get => m_AdaptDifferentBlock; set => m_AdaptDifferentBlock = value; }
		public bool AdaptDifferentLayer { get => m_AdaptDifferentLayer; set => m_AdaptDifferentLayer = value; }
		public bool AdaptGround { get => m_AdaptGround; set => m_AdaptGround = value; }

		// Ser
		[SerializeField] private List<AdaptiveData> m_Prefabs = new List<AdaptiveData>();
		[SerializeField] private bool m_AdaptDifferentBlock = true;
		[SerializeField] private bool m_AdaptDifferentLayer = true;
		[SerializeField] private bool m_AdaptGround = true;


		#endregion




		#region --- MSG ---


		public override string GetPaletteLabel () => "A";


		public override bool AllowRotate () => false;


		public override void OnBlockLoaded (JujubeRenderer renderer, JujubeBlock block) {
			
			if (block != null && renderer != null && renderer.Mode == JujubeRendererMode.Release) {
				int layerIndex = transform.parent.GetSiblingIndex();
				block.RotZ = 0;
				transform.localRotation = Quaternion.identity;

				// Reload
				int index = GetPrefabIndexFromSurrounded(renderer, block, layerIndex);
				if (transform.childCount > 0 && transform.GetChild(0).name != index.ToString()) {
					DestroyAllChirldrenImmediate(transform);
					if (index >= 0 && index < m_Prefabs.Count) {
						var prefab = m_Prefabs[index];
						if (prefab != null && prefab.Prefab != null) {
							var tf = Instantiate(prefab.Prefab, transform).transform;
							tf.name = index.ToString();
							tf.localPosition = Vector3.zero;
							tf.localRotation = Quaternion.identity;
							tf.localScale = Vector3.one;
						}
					}
				}
			}

			// Destroy
			DestroyImmediate(this, false);

		}


		#endregion




		#region --- LGC ---


		private int GetPrefabIndexFromSurrounded (JujubeRenderer renderer, JujubeBlock block, int layerIndex) {

			// Get Surround Code
			short surroundCode = 0;
			bool hasBlock;
			int targetLayerIndex = m_AdaptDifferentLayer ? -1 : layerIndex;
			for (int posIndex = 0; posIndex < 6; posIndex++) {
				var _pos = block.Position + ADP_POS_XYZ[posIndex];
				if (_pos.y < 0) {
					hasBlock = m_AdaptGround;
				} else {
					var _block = renderer.GetBlockIndex(_pos, targetLayerIndex, false).block;
					hasBlock = _block != null;
					if (!m_AdaptDifferentBlock && hasBlock && _block.Index != block.Index) {
						hasBlock = false;
					}
				}
				surroundCode = SetBitValue(surroundCode, posIndex, hasBlock);
			}

			// Get Prefab Index
			int prefabCount = Prefabs.Count;
			int resultIndex = 0;
			for (int i = 0; i < prefabCount; i++) {
				var prefab = Prefabs[i];
				if (CodeFit(prefab.Adaptive, surroundCode)) {
					resultIndex = i;
					break;
				}
			}
			return resultIndex;
		}


		#endregion




		#region --- UTL ---


		private static bool GetBit (short value, int index) {
			if (index < 0 || index > 15) { return false; }
			short val = (short)(1 << index);
			return (value & val) == val;
		}


		private static short SetBitValue (short value, int index, bool bitValue) {
			if (index < 0 || index > 15) { return value; }
			short val = (short)(1 << index);
			return (short)(bitValue ? (value | val) : (value & ~val));
		}


		private static bool CodeFit (short center, short surround) {
			for (int i = 0; i < 6; i++) {
				if (!GetBit(center, i + 6)) { continue; }
				if (GetBit(center, i) != GetBit(surround, i)) { return false; }
			}
			return true;
		}


		private static void DestroyAllChirldrenImmediate (Transform target) {
			int childCount = target.childCount;
			for (int i = 0; i < childCount; i++) {
				DestroyImmediate(target.GetChild(0).gameObject, false);
			}
		}


		#endregion




	}
}




#if UNITY_EDITOR
namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;



	[CustomEditor(typeof(AdaptiveBlock)), DisallowMultipleComponent]
	public class AdaptiveBlock_Inspector : Editor {




		// VAR
		private static readonly Vector2[] ADP_POS_XZ = {
			new Vector2(3.25f, 1.5f), // D
			new Vector2(3.25f, 0.5f), // U
			new Vector2(1, 2), // B
			new Vector2(1, 0), // F
			new Vector2(0, 1), // L
			new Vector2(2, 1), // R
		};
		private static GUIStyle LargeFontButtonStyle => _LargeFontButtonStyle != null ? _LargeFontButtonStyle : (_LargeFontButtonStyle = new GUIStyle(GUI.skin.button) {
			fontSize = 16,
			alignment = TextAnchor.MiddleCenter,
		});
		private static GUIStyle SmallFontButtonStyle => _SmallFontButtonStyle != null ? _SmallFontButtonStyle : (_SmallFontButtonStyle = new GUIStyle(GUI.skin.button) {
			fontSize = 6,
			alignment = TextAnchor.MiddleCenter,
		});
		private static GUIStyle _LargeFontButtonStyle = null;
		private static GUIStyle _SmallFontButtonStyle = null;



		// MSG
		public override void OnInspectorGUI () {


			var aTarget = target as AdaptiveBlock;

			Space(4);

			// Only Adapt Same Block
			LayoutH(() => {
				GUI.Label(GUIRect(0, 18), "Adapt Different Block");
				aTarget.AdaptDifferentBlock = GUI.Toggle(
					GUIRect(18, 18),
					aTarget.AdaptDifferentBlock,
					GUIContent.none
				);
				Space(64);
			});

			// Only Adapt Same Layer
			LayoutH(() => {
				GUI.Label(GUIRect(0, 18), "Adapt Different Layer");
				aTarget.AdaptDifferentLayer = GUI.Toggle(
					GUIRect(18, 18),
					aTarget.AdaptDifferentLayer,
					GUIContent.none
				);
				Space(64);
			});

			// Adapt Ground
			LayoutH(() => {
				GUI.Label(GUIRect(0, 18), "Adapt Ground");
				aTarget.AdaptGround = GUI.Toggle(
					GUIRect(18, 18),
					aTarget.AdaptGround,
					GUIContent.none
				);
				Space(64);
			});

			// Prefabs
			Space(14);

			GUI.Label(GUIRect(0, 18), "Prefabs");
			// Content
			const float ITEM_SIZE = 72;
			int deleteIndex = -1;
			int swipeIndexA = -1;
			int swipeIndexB = -1;
			int prefabCount = aTarget.Prefabs.Count;
			if (prefabCount > 0) {
				ColorBlock(GUIRect(0, 1), new Color(0.5f, 0.5f, 0.5f, 0.3f));
				Space(6);
			}
			for (int i = 0; i < prefabCount; i++) {
				var prefab = aTarget.Prefabs[i];

				// Adps
				LayoutH(() => {
					Space(6);
					var _rect = GUIRect(ITEM_SIZE, ITEM_SIZE);
					GUI.Label(
						new Rect(_rect.x + _rect.width / 3f, _rect.y + _rect.width / 3f, _rect.width / 3f, _rect.height / 3f),
						i.ToString(), EditorStyles.centeredGreyMiniLabel
					);
					GUI_AdpButton(_rect, prefab, AdaptiveBlock.AdaptivePosition.Down);
					GUI_AdpButton(_rect, prefab, AdaptiveBlock.AdaptivePosition.Up);
					GUI_AdpButton(_rect, prefab, AdaptiveBlock.AdaptivePosition.Back);
					GUI_AdpButton(_rect, prefab, AdaptiveBlock.AdaptivePosition.Forward);
					GUI_AdpButton(_rect, prefab, AdaptiveBlock.AdaptivePosition.Left);
					GUI_AdpButton(_rect, prefab, AdaptiveBlock.AdaptivePosition.Right);
					Space(46);

					// Prefab
					LayoutV(() => {

						GUIRect(1, 0);

						prefab.Prefab = (GameObject)EditorGUI.ObjectField(
							GUIRect(0, 18),
							prefab.Prefab, typeof(GameObject), false
						);
						Space(6);

						LayoutH(() => {
							GUIRect(0, 18);
							// Up
							var oldE = GUI.enabled;
							GUI.enabled = i > 0;
							if (GUI.Button(GUIRect(20, 18), "▲", SmallFontButtonStyle) && i > 0) {
								swipeIndexA = i;
								swipeIndexB = i - 1;
							}

							// Down
							GUI.enabled = i < prefabCount - 1;
							if (GUI.Button(GUIRect(20, 18), "▼", SmallFontButtonStyle) && i < prefabCount - 1) {
								swipeIndexA = i;
								swipeIndexB = i + 1;
							}
							GUI.enabled = oldE;

							// Delete
							if (GUI.Button(GUIRect(20, 18), "×")) {
								deleteIndex = i;
							}
						});
						GUIRect(1, 0);
					});


				});

				// Line
				Space(6);
				ColorBlock(GUIRect(0, 1), new Color(0.5f, 0.5f, 0.5f, 0.3f));
				Space(3);
			}

			// Index Action
			if (swipeIndexA >= 0 && swipeIndexB >= 0 && swipeIndexA < prefabCount && swipeIndexB < prefabCount) {
				// Swipe Item
				var temp = aTarget.Prefabs[swipeIndexA];
				aTarget.Prefabs[swipeIndexA] = aTarget.Prefabs[swipeIndexB];
				aTarget.Prefabs[swipeIndexB] = temp;
			} else if (deleteIndex >= 0) {
				// Delete Item
				EditorApplication.delayCall += () => {
					if (EditorUtility.DisplayDialog("Delete", $"Delete Item at {deleteIndex}?", "Delete", "Cancel")) {
						aTarget.Prefabs.RemoveAt(deleteIndex);
					}
				};
			}

			// Bar
			LayoutH(() => {
				GUIRect(0, 18);
				if (GUI.Button(GUIRect(60, 18), "+")) {
					aTarget.Prefabs.Add(new AdaptiveBlock.AdaptiveData());
				}
			});
			Space(2);

			if (GUI.changed) {
				EditorUtility.SetDirty(target);
			}


			// --- Func ---
			void GUI_AdpButton (Rect rect, AdaptiveBlock.AdaptiveData prefab, AdaptiveBlock.AdaptivePosition pos) {
				rect.width /= 3f;
				rect.height /= 3f;
				var posXY = ADP_POS_XZ[(int)pos];
				rect.x += posXY.x * rect.width;
				rect.y += posXY.y * rect.height;
				var state = prefab.GetAdaptiveState(pos);
				if (GUI.Button(
					rect,
					state == AdaptiveBlock.AdaptiveState.Solid ? "●" :
					state == AdaptiveBlock.AdaptiveState.Air ? "○" :
					string.Empty,
					LargeFontButtonStyle
				)) {
					prefab.SetAdaptiveState(pos, (AdaptiveBlock.AdaptiveState)(((int)state + 1) % 3));
				}
				if (state != AdaptiveBlock.AdaptiveState.Air) {
					GUI.Label(rect, pos.ToString()[0].ToString(), EditorStyles.centeredGreyMiniLabel);
				}
			}
		}



		#region --- UTL ---


		private static void LayoutV (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2)
				};
			}
			if (style != null) {
				GUILayout.BeginVertical(style);
			} else {
				GUILayout.BeginVertical();
			}
			action();
			GUILayout.EndVertical();
		}


		private static void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2)
				};
			}
			if (style != null) {
				GUILayout.BeginHorizontal(style);
			} else {
				GUILayout.BeginHorizontal();
			}
			action();
			GUILayout.EndHorizontal();
		}


		private static void LayoutF (System.Action action, string label, ref bool open, bool box = false, GUIStyle style = null) {
			bool _open = open;
			LayoutV(() => {
				_open = GUILayout.Toggle(
					_open,
					label,
					GUI.skin.GetStyle("foldout"),
					GUILayout.ExpandWidth(true),
					GUILayout.Height(18)
				);
				if (_open) {
					action();
				}
			}, box, style);
			Space(4);
			open = _open;
		}


		private static void Space (float space = 4f) {
			GUILayout.Space(space);
		}


		private static Rect GUIRect (float width, float height) {
			return GUILayoutUtility.GetRect(
				width, height,
				GUILayout.ExpandWidth(width == 0), GUILayout.ExpandHeight(height == 0)
			);
		}


		private static void ColorBlock (Rect rect, Color color) {
			var oldC = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, Texture2D.whiteTexture);
			GUI.color = oldC;
		}


		#endregion

	}
}
#endif