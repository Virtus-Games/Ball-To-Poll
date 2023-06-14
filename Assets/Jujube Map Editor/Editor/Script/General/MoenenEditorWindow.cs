namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;



	public class MoenenEditorWindow : EditorWindow {


		protected static Rect GUIRect (float width, float height) {
			return GUILayoutUtility.GetRect(
				width, height,
				GUILayout.ExpandWidth(width == 0), GUILayout.ExpandHeight(height == 0)
			);
		}


		protected static void LayoutV (System.Action action, bool box = false, GUIStyle style = null) {
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


		protected static void LayoutH (System.Action action, bool box = false, GUIStyle style = null) {
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


		protected static void LayoutF (System.Action action, string label, ref bool open, bool box = false, GUIStyle style = null) {
			bool _open = open;
			LayoutV(() => {
				var rect = GUIRect(0, 18);
				if (style != null && style.padding.left > 4) {
					rect.x -= style.padding.left - 4;
					rect.width += style.padding.left - 4;
				}
				_open = GUI.Toggle(
					rect,
					_open, label,
					GUI.skin.GetStyle("foldout")
				);
				if (_open) {
					action();
				}
			}, box, style);
			Space(4);
			open = _open;
		}


		protected static void Space (float space = 4f) {
			GUILayout.Space(space);
		}


		protected static string GetDisplayString (string str, int maxLength) {
			return str.Length > maxLength ? str.Substring(0, maxLength - 3) + "..." : str;
		}


		protected static void ColorBlock (Rect rect) {
			ColorBlock(rect, new Color(1, 1, 1, 0.1f));
		}


		protected static void ColorBlock (Rect rect, Color color) {
			var oldC = GUI.color;
			GUI.color = color;
			GUI.DrawTexture(rect, Texture2D.whiteTexture);
			GUI.color = oldC;
		}


	}
}
