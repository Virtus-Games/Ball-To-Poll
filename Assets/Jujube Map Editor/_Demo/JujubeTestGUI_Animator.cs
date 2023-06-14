namespace JujubeMapEditor.Test {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using JujubeMapEditor.Core;


	public class JujubeTestGUI_Animator : MonoBehaviour {
		private void Start () {
			Application.targetFrameRate = 999;

		}
		private void OnGUI () {
			float width = 128;
			float height = 64;
			float y = 12;
			float x = 12;
			if (GUI.Button(new Rect(x, y, width, height), "◀")) {
				var ani = GetComponent<JujubeAnimator>();
				if (ani != null) {
					ani.PlayInverse();
				}
			}
			x += width + 6;
			if (GUI.Button(new Rect(x, y, width, height), "▶")) {
				var ani = GetComponent<JujubeAnimator>();
				if (ani != null) {
					ani.Play();
				}
			}
		}
	}
}