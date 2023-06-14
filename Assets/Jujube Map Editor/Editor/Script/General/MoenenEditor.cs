namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;



	public class MoenenEditor {


		// VAR
		public const int JUJUBE_HANDLE_ID = 19940516;
		private readonly static Vector3[] WireCubeLineCaches = new Vector3[24];
		private readonly static Vector3[] SolidRectangleCaches = new Vector3[4];


		// GUI
		protected static Rect GUIRect (float width, float height) {
			return GUILayoutUtility.GetRect(
				width, height,
				GUILayout.ExpandWidth(width == 0), GUILayout.ExpandHeight(height == 0)
			);
		}


		protected static void Link (int width, int height, string label, string link) {
			var buttonRect = GUIRect(width, height);
			if (GUI.Button(buttonRect, label, new GUIStyle(GUI.skin.label) {
				wordWrap = true,
				normal = new GUIStyleState() {
					textColor = new Color(86f / 256f, 156f / 256f, 214f / 256f),
					background = null,
					scaledBackgrounds = null,
				}
			})) {
				Application.OpenURL(link);
			}
			EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
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


		protected static void Space (float space = 4f) {
			GUILayout.Space(space);
		}


		protected static bool ColorfulButton (Rect rect, string label, Color color, GUIStyle style = null) {
			Color oldColor = GUI.color;
			GUI.color = color;
			bool pressed = style == null ? GUI.Button(rect, label) : GUI.Button(rect, label, style);
			GUI.color = oldColor;
			return pressed;
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


		protected static void HandlesLayoutV (Rect rect, System.Action action, GUIStyle style = null) {
			Handles.BeginGUI();
			GUILayout.BeginArea(rect);
			LayoutV(action, false, style);
			GUILayout.EndArea();
			Handles.EndGUI();
		}


		protected static void HandlesLayoutH (Rect rect, System.Action action, GUIStyle style = null) {
			Handles.BeginGUI();
			GUILayout.BeginArea(rect);
			LayoutH(action, false, style);
			GUILayout.EndArea();
			Handles.EndGUI();
		}


		protected static string TextField (Rect rect, string text, ref bool focusing, GUIStyle style = null) {
			const string CTRL_NAME = "Jujube TextField Name";
			GUI.SetNextControlName(CTRL_NAME);
			text = GUI.TextField(rect, text, style != null ? style : GUI.skin.textField);
			focusing = focusing || GUI.GetNameOfFocusedControl() == CTRL_NAME;
			return text;
		}


		// Misc
		protected static string GetDisplayString (string str, int maxLength) {
			return str.Length > maxLength ? str.Substring(0, maxLength - 3) + "..." : str;
		}


		// Handles Draw
		protected static void DrawCoord (Vector3 min, Vector3 max, float cellSize, Matrix4x4 matrix, bool edgeX = true, bool edgeZ = true) {
			Vector3 a = new Vector3(0, min.y, 0);
			Vector3 b = new Vector3(0, max.y, 0);
			a.z = min.z;
			b.z = max.z;
			float l = edgeX ? min.x : min.x + cellSize;
			float r = edgeX ? max.x + cellSize / 2f : max.x + cellSize / 2f - cellSize;
			for (float x = l; x < r; x += cellSize) {
				a.x = x;
				b.x = x;
				Handles.DrawLine(
					matrix.MultiplyPoint3x4(a),
					matrix.MultiplyPoint3x4(b)
				);
			}
			a.x = min.x;
			b.x = max.x;
			float d = edgeZ ? min.z : min.z + cellSize;
			float u = edgeZ ? max.z + cellSize / 2f : max.z + cellSize / 2f - cellSize;
			for (float z = d; z < u; z += cellSize) {
				a.z = z;
				b.z = z;
				Handles.DrawLine(
					matrix.MultiplyPoint3x4(a),
					matrix.MultiplyPoint3x4(b)
				);
			}
		}


		protected static void DrawCube (Vector3 pos, Quaternion rot, float size, Color color, float alpha) {
			color.a *= alpha;
			var oldC = Handles.color;
			Handles.color = color;
			Handles.CubeHandleCap(0, pos, rot, size, EventType.Repaint);
			Handles.color = oldC;
		}


		protected static void DrawWireCube (Vector3 min, Vector3 max, Matrix4x4 matrix, Color color, float alpha, bool dotted = false) {

			color.a *= alpha;

			// Set Caches
			WireCubeLineCaches[0] = WireCubeLineCaches[8] = WireCubeLineCaches[22] =
				matrix.MultiplyPoint3x4(new Vector3(min.x, min.y, min.z));
			WireCubeLineCaches[4] = WireCubeLineCaches[14] = WireCubeLineCaches[23] =
				matrix.MultiplyPoint3x4(new Vector3(min.x, min.y, max.z));
			WireCubeLineCaches[2] = WireCubeLineCaches[9] = WireCubeLineCaches[20] =
				matrix.MultiplyPoint3x4(new Vector3(max.x, min.y, min.z));
			WireCubeLineCaches[6] = WireCubeLineCaches[15] = WireCubeLineCaches[21] =
				matrix.MultiplyPoint3x4(new Vector3(max.x, min.y, max.z));

			WireCubeLineCaches[1] = WireCubeLineCaches[10] = WireCubeLineCaches[16] =
				matrix.MultiplyPoint3x4(new Vector3(min.x, max.y, min.z));
			WireCubeLineCaches[5] = WireCubeLineCaches[12] = WireCubeLineCaches[17] =
				matrix.MultiplyPoint3x4(new Vector3(min.x, max.y, max.z));
			WireCubeLineCaches[3] = WireCubeLineCaches[11] = WireCubeLineCaches[18] =
				matrix.MultiplyPoint3x4(new Vector3(max.x, max.y, min.z));
			WireCubeLineCaches[7] = WireCubeLineCaches[13] = WireCubeLineCaches[19] =
				matrix.MultiplyPoint3x4(new Vector3(max.x, max.y, max.z));

			// Draw
			var oldC = Handles.color;
			Handles.color = color;
			if (dotted) {
				Handles.DrawDottedLines(WireCubeLineCaches, 2.4f);
			} else {
				Handles.DrawLines(WireCubeLineCaches);
			}
			Handles.color = oldC;
		}


		protected static void DrawQuad (Vector3 pos, Vector3 up, Vector3 forward, Vector3 normal, float size, Color color, float alpha) {
			color.a *= alpha;
			float angle = Vector3.Angle(normal, up);
			Vector3 normalAlt = angle > 45f && angle < 135f ? up : forward;
			size /= 2f;
			normal *= size;
			var rot = Quaternion.LookRotation(normal, normalAlt);
			SolidRectangleCaches[0] = pos + normal + rot * new Vector3(-size, -size, 0f);
			SolidRectangleCaches[1] = pos + normal + rot * new Vector3(size, -size, 0f);
			SolidRectangleCaches[2] = pos + normal + rot * new Vector3(size, size, 0f);
			SolidRectangleCaches[3] = pos + normal + rot * new Vector3(-size, size, 0f);

			// Draw
			var oldC = Handles.color;
			Handles.color = color;
			Handles.DrawSolidRectangleWithOutline(SolidRectangleCaches, color, color);
			Handles.color = oldC;
		}


		// Handles
		protected static bool MouseInJujubeHandle () => HandleUtility.nearestControl >= JUJUBE_HANDLE_ID && HandleUtility.nearestControl < JUJUBE_HANDLE_ID + 16;


		protected static Vector3 BoxPositionHandle (Vector3 pos, Vector3 min, Vector3 max, Vector3 forward, Vector3 up, Vector3 right, float cellSize, Vector3 cameraPos, Color[] wireColors, Color[] faceColors) {

			min *= cellSize;
			max *= cellSize;
			var size = max - min;
			var handleSize = HandleUtility.GetHandleSize(pos) * 0.2f;
			Vector3? newPos;

			// D
			newPos = PlanarHandle(
				0, pos,
				0.5f * (size.y + cellSize) * -up,
				-up, right, forward,
				cameraPos, handleSize,
				wireColors[1], faceColors[1]
			 );
			if (newPos.HasValue) { return newPos.Value; }

			// U
			newPos = PlanarHandle(
				1, pos,
				0.5f * (size.y + cellSize) * up,
				up, right, forward,
				cameraPos, handleSize,
				wireColors[1], faceColors[1]
			);
			if (newPos.HasValue) { return newPos.Value; }

			// B
			newPos = PlanarHandle(
				2, pos,
				0.5f * (size.z + cellSize) * -forward,
				-forward, up, right,
				cameraPos, handleSize,
				wireColors[2], faceColors[2]
			);
			if (newPos.HasValue) { return newPos.Value; }

			// F
			newPos = PlanarHandle(
				3, pos,
				0.5f * (size.z + cellSize) * forward,
				forward, up, right,
				cameraPos, handleSize,
				wireColors[2], faceColors[2]
			);
			if (newPos.HasValue) { return newPos.Value; }

			// L
			newPos = PlanarHandle(
				4, pos,
				0.5f * (size.x + cellSize) * -right,
				-right, up, forward,
				cameraPos, handleSize,
				wireColors[0], faceColors[0]
			);
			if (newPos.HasValue) { return newPos.Value; }

			// R
			newPos = PlanarHandle(
				5, pos,
				0.5f * (size.x + cellSize) * right,
				right, up, forward,
				cameraPos, handleSize,
				wireColors[0], faceColors[0]
			);
			if (newPos.HasValue) { return newPos.Value; }

			return pos;
		}


		protected static Vector3 QuadPositionHandle (Vector3 pos, Vector3 min, Vector3 max, Vector3 forward, Vector3 up, Vector3 right, float cellSize, Vector3 cameraPos, Color[] wireColors, Color[] faceColors) {

			min *= cellSize;
			max *= cellSize;
			var size = max - min;
			var handleSize = HandleUtility.GetHandleSize(pos) * 0.3f;
			Vector3? newPosArrow, newPosQuad;


			// Arrow
			var oldC = Handles.color;
			Handles.color = wireColors[1];
			var offset =
				0.5f * (size.y + cellSize - handleSize * 4f) * -up +
				handleSize * 2f * Vector3.ProjectOnPlane(pos - cameraPos, up).normalized;
			EditorGUI.BeginChangeCheck();
			newPosArrow = Handles.Slider(
				JUJUBE_HANDLE_ID,
				pos + offset,
				up,
				handleSize,
				Handles.ConeHandleCap,
				0f
			) - offset;
			Handles.color = oldC;
			if (EditorGUI.EndChangeCheck()) {
				newPosArrow = pos + Vector3.Project(newPosArrow.Value - pos, up);
			} else {
				newPosArrow = null;
			}

			// D
			newPosQuad = PlanarHandle(
				1, pos,
				0.5f * (size.y + cellSize) * -up,
				up, right, forward,
				null, handleSize,
				wireColors[1], faceColors[1]
			);

			// Result
			if (newPosArrow.HasValue) {
				return newPosArrow.Value;
			}
			if (newPosQuad.HasValue) {
				return newPosQuad.Value;
			}
			return pos;
		}


		// LGC
		private static Vector3? PlanarHandle (int id, Vector3 pos, Vector3 offset, Vector3 normal, Vector3 dirA, Vector3 dirB, Vector3? cameraPos, float handleSize, Color wireColor, Color faceColor) {
			var pivot = pos + offset;
			if (!cameraPos.HasValue || Vector3.Angle(normal, cameraPos.Value - pivot) < 90f) {

				// Draw the Wire
				SolidRectangleCaches[0] = pivot + (dirA + dirB) * handleSize * 0.618f;
				SolidRectangleCaches[1] = pivot + (-dirA + dirB) * handleSize * 0.618f;
				SolidRectangleCaches[2] = pivot + (-dirA - dirB) * handleSize * 0.618f;
				SolidRectangleCaches[3] = pivot + (dirA - dirB) * handleSize * 0.618f;
				Handles.DrawSolidRectangleWithOutline(SolidRectangleCaches, faceColor, Color.clear);

				// Do the Handle
				var oldC = Handles.color;
				Handles.color = wireColor;
				EditorGUI.BeginChangeCheck();
				var newPos = Handles.Slider2D(
					JUJUBE_HANDLE_ID + id,
					pivot,
					Vector3.zero,
					normal,
					dirA,
					dirB,
					handleSize,
					Handles.RectangleHandleCap,
					Vector2.zero,
					false
				);
				Handles.color = oldC;

				if (EditorGUI.EndChangeCheck()) {
					return pos + newPos - pivot;
				}

			}
			return null;
		}


	}
}
