namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;



	[CustomEditor(typeof(JujubeAnimator), true)]
	public class JujubeAnimator_Inspector : MoenenInspector {



		// Const
		private const float LABEL_WIDTH = 120;

		// Short
		private static Texture2D JujubIcon => _JujubIcon != null ? _JujubIcon : (_JujubIcon = EditorUtil.GetImage(EditorGUIUtility.isProSkin ? "Icon.png" : "Icon Dark.png"));
		private static Texture2D _JujubIcon = null;

		// Data
		private JujubeRenderer Renderer = null;


		// MSG
		private void OnEnable () {
			Renderer = (target as JujubeAnimator).GetComponent<JujubeRenderer>();
		}


		public override void OnInspectorGUI () {
			GUI_ControlPanel();
			base.OnInspectorGUI();
		}


		private void GUI_ControlPanel () {
			bool isPlaying = EditorApplication.isPlaying;
			bool isRendererEditing = Renderer != null && JujubeScene.EditingRenderer == Renderer;
			if (!isRendererEditing) {
				var jTarget = target as JujubeAnimator;
				if (isPlaying) {
					// Play Mode
					Space(4);
					LayoutH(() => {
						GUIRect(0, 18);
						// Inplay
						if (GUI.Button(GUIRect(42, 18), "◀")) {
							jTarget.PlayInverse();
						}
						// Play
						if (GUI.Button(GUIRect(42, 18), "▶")) {
							jTarget.Play();
						}
						// Pause
						if (GUI.Button(GUIRect(42, 18), "∥")) {
							jTarget.Stop();
						}
						GUIRect(0, 18);
					});
					Space(8);
				}
			}
		}


	}
}