namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;


	public class MoenenInspector : Editor {




		protected virtual string[] ExcludingParams => _ExcludingParams;
		private readonly static string[] _ExcludingParams = { "m_Script", };
		protected static GUIStyle RichHelpBox {
			get {
				if (_RichHelpBox == null) {
					_RichHelpBox = new GUIStyle(EditorStyles.helpBox) {
						richText = true,
						alignment = TextAnchor.MiddleCenter,
					};
				}
				return _RichHelpBox;
			}
		}
		private static GUIStyle _RichHelpBox = null;
		private static Texture2D WhitePixel {
			get {
				if (_WhitePixel == null) {
					_WhitePixel = Texture2D.whiteTexture;
				}
				return _WhitePixel;
			}
		}
		private static Texture2D _WhitePixel = null;
		private readonly static Dictionary<string, Texture2D> ImageMap = new Dictionary<string, Texture2D>();


		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, ExcludingParams);
			serializedObject.ApplyModifiedProperties();
		}



		protected static void LayoutV (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2),
					margin = new RectOffset(0, 0, 0, 0),
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


		protected static void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
			if (box) {
				style = new GUIStyle(GUI.skin.box) {
					padding = new RectOffset(6, 6, 2, 2),
					margin = new RectOffset(0, 0, 0, 0),
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


		protected static void LayoutF (System.Action action, string label, ref bool open, bool box = false, GUIStyle style = null) {
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


		protected static void LayoutF_Button (System.Action action, System.Action<Rect> labelAction, System.Action buttonAction, ref bool open, bool box = false, GUIStyle style = null) {
			bool _open = open;
			LayoutV(() => {
				LayoutH(() => {
					var rect = GUIRect(0, 18);
					_open = GUI.Toggle(rect, _open, GUIContent.none, GUI.skin.GetStyle("foldout"));
					rect.x += 18;
					rect.width -= 18;
					labelAction(rect);
					if (GUI.Button(GUIRect(18, 18), "…", EditorStyles.boldLabel)) {
						buttonAction();
					}
				}, false, GUIStyle.none);
				if (_open) {
					action();
				}
			}, box, style);
			Space(4);
			open = _open;
		}


		protected static void LayoutF_Double (System.Action action, string label, ref bool open, ref bool alt, bool box = false, GUIStyle style = null) {
			bool _open = open;
			bool _alt = alt;
			LayoutV(() => {
				GUILayout.BeginHorizontal();
				_alt = EditorGUI.Toggle(GUIRect(18, 18), _alt);
				Space(2);
				_open = GUILayout.Toggle(
					_open,
					label,
					GUI.skin.GetStyle("foldout"),
					GUILayout.ExpandWidth(true),
					GUILayout.Height(18)
				);
				GUILayout.EndHorizontal();
				if (_open) {
					action();
				}
			}, box, style);
			Space(4);
			open = _open;
			alt = _alt;
		}


		protected static void Space (float space = 4f) {
			GUILayout.Space(space);
		}


		protected static Rect GUIRect (float width, float height) {
			return GUILayoutUtility.GetRect(
				width, height,
				GUILayout.ExpandWidth(width == 0), GUILayout.ExpandHeight(height == 0)
			);
		}


		protected static void DragGUI<T> (Rect rect, string msg, string ext, System.Func<T, bool> dragPerform, System.Action done = null) where T : Object {
			GUI.Label(rect, msg, RichHelpBox);
			switch (Event.current.type) {
				case EventType.DragUpdated:
					if (rect.Contains(Event.current.mousePosition)) {
						if (DragAndDrop.objectReferences.Length > 0) {
							DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
						}
					}
					break;
				case EventType.DragPerform:
					if (rect.Contains(Event.current.mousePosition)) {
						foreach (var obj in DragAndDrop.objectReferences) {
							if (obj is T tObj) {
								bool _break = dragPerform(tObj);
								if (_break) { break; }
							} else {
								var path = EditorUtil.FixedRelativePath(AssetDatabase.GetAssetPath(obj));
								if (AssetDatabase.IsValidFolder(path)) {
									var files = EditorUtil.GetFilesIn(path, false, $"*{ext}");
									foreach (var file in files) {
										var prefabPath = EditorUtil.FixedRelativePath(file.FullName);
										var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) as T;
										if (prefab == null) { continue; }
										bool _break = dragPerform(prefab);
										if (_break) { break; }
									}
								}
							}
						}
						done?.Invoke();
						DragAndDrop.AcceptDrag();
					}
					break;
			}
		}


		protected static T CreateAsset<T> (string basicName, string errorMsg = "") where T : ScriptableObject {
			try {
				// Get Path
				string path = EditorUtil.CombinePaths("Assets", $"{basicName}.asset");
				int index = 1;
				while (EditorUtil.FileExists(path)) {
					path = EditorUtil.CombinePaths("Assets", $"{basicName} ({index}).asset");
					index++;
				}
				// Create Asset
				var asset = CreateInstance<T>();
				AssetDatabase.CreateAsset(asset, path);
				return asset;
			} catch (System.Exception ex) {
				if (!string.IsNullOrEmpty(errorMsg)) {
					Debug.LogWarning($"{errorMsg}\n{ex.Message}");
				}
			}
			return null;
		}


		protected static Texture2D GetJujubeImage (string fileName) {
			if (ImageMap.ContainsKey(fileName)) {
				return ImageMap[fileName];
			} else {
				var image = EditorUtil.GetImage(fileName);
				if (image != null) {
					ImageMap.Add(fileName, image);
				}
				return image;
			}
		}


		protected static void ColorBlock (Rect rect, Color color) {
			var oldC = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, WhitePixel);
			GUI.color = oldC;
		}


	}
}