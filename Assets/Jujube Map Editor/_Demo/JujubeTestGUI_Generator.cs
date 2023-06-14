namespace JujubeMapEditor.Test {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using JujubeMapEditor.Core;



	[ExecuteInEditMode]
	public class JujubeTestGUI_Generator : MonoBehaviour {



		// Ser-Api
		public JujubeGenerator Generator = null;
		public float GUIScale = 3f;
		public bool ShowIteration = true;
		public bool ShowIterationRadius = true;
		public bool ShowGroundHeightMin = false;
		public bool ShowGroundHeightMax = false;
		public bool ShowWaterHeight = false;



		private void Reset () {
			Generator = GetComponent<JujubeGenerator>();
		}


		private void OnGUI () {

			if (Generator == null) { return; }

			const int WIDTH = 120;
			const int HEIGHT = 22;
			const int GAP = 0;
			const int X0 = 12;
			const int X1 = X0 + WIDTH;
			int y = 12;

			var oldM = GUI.matrix;
			GUI.matrix *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * GUIScale);

			if (ShowIteration) {
				int dot = Mathf.Clamp(Generator.Ground_Iteration, 0, 12);
				GUI.Label(new Rect(X0, y, WIDTH, HEIGHT), "Iteration");
				GUI.Label(new Rect(X1, y, WIDTH, HEIGHT), new string('■', dot) + new string('□', 12 - dot));
				y += HEIGHT + GAP;
			}

			if (ShowIterationRadius) {
				int dot = Mathf.Clamp((int)Generator.Ground_IterationRadius, 0, 12);
				GUI.Label(new Rect(X0, y, WIDTH, HEIGHT), "Iteration Radius");
				GUI.Label(new Rect(X1, y, WIDTH, HEIGHT), new string('■', dot) + new string('□', 12 - dot));
				y += HEIGHT + GAP;
			}

			if (ShowGroundHeightMin) {
				int dot = Mathf.Clamp(Generator.Ground_MinHeight, 0, 12);
				GUI.Label(new Rect(X0, y, WIDTH, HEIGHT), "Ground Height Min");
				GUI.Label(new Rect(X1, y, WIDTH, HEIGHT), new string('■', dot) + new string('□', 12 - dot));
				y += HEIGHT + GAP;
			}

			if (ShowGroundHeightMax) {
				int dot = Mathf.Clamp(Generator.Ground_MaxHeight, 0, 12);
				GUI.Label(new Rect(X0, y, WIDTH, HEIGHT), "Ground Height Max");
				GUI.Label(new Rect(X1, y, WIDTH, HEIGHT), new string('■', dot) + new string('□', 12 - dot));
				y += HEIGHT + GAP;
			}

			if (ShowWaterHeight) {
				int dot = Mathf.Clamp(Generator.Water_Height, 0, 12);
				GUI.Label(new Rect(X0, y, WIDTH, HEIGHT), "Water Height");
				GUI.Label(new Rect(X1, y, WIDTH, HEIGHT), new string('■', dot) + new string('□', 12 - dot));
				//y += HEIGHT + GAP;
			}

			GUI.matrix = oldM;
		}


	}
}