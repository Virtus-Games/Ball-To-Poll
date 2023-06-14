namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using System.Diagnostics;
	using UnityEngine;



	public class JuicyAnimator : JujubeAnimator {




		#region --- VAR ---


		// Const
		private static readonly Vector3 CENTER_POS = new Vector3(0.5f, 0f, 0.5f);

		// Ser
		[Range(0f, 1f), SerializeField] private float m_BlockEase = 0.3f;
		[SerializeField] private AnimationCurve m_BlockEaseCurve = AnimationCurve.Linear(0, 0, 1, 1);
		[SerializeField] private bool m_AlwaysActiveBlocks = false;

		// Data
		private Vector3 BlockLocalScale = Vector3.one;


		#endregion




		#region --- PRT ---


		protected override float GetSpreadAmount (Vector3 pos) => J_Remap(
			0f, 1.119f, 0f, 1f - m_BlockEase,
			Vector3.Distance(CENTER_POS, pos)
		);


		protected override int SetSpreadAmount (Transform blockTF, float amountOffset) {
			if (amountOffset > 0f) {
				if (!m_AlwaysActiveBlocks) {
					if (!blockTF.gameObject.activeSelf) {
						blockTF.gameObject.SetActive(true);
					}
				}
				float x01 = amountOffset / m_BlockEase;
				if (x01 > 1f || Mathf.Approximately(x01, 1f)) {
					x01 = 1f;
				}
				blockTF.localScale = BlockLocalScale * Mathf.Max(
					m_BlockEaseCurve.Evaluate(x01), 0f
				);
				return Mathf.Approximately(x01, 1f) ? 1 : 0;
			} else {
				if (m_AlwaysActiveBlocks) {
					blockTF.localScale = Vector3.zero;
				} else {
					if (blockTF.gameObject.activeSelf) {
						blockTF.gameObject.SetActive(false);
					}
				}
				return -1;
			}
		}


		protected override void BeforePlay () {
			base.BeforePlay();
			BlockLocalScale = GetCellScale();
		}


		#endregion




		#region --- LGC ---


		private float J_Remap (float l, float r, float newL, float newR, float t) => l == r ? l : Mathf.LerpUnclamped(newL, newR, (t - l) / (r - l));


		private Vector3 GetCellScale () {
			var renderer = GetComponent<JujubeRenderer>();
			if (renderer != null) {
				return Vector3.one * (
					renderer.PrefabScale == JujubePrefabScaleMode.CellSize || renderer.PrefabScale == JujubePrefabScaleMode.LocalScaleAndCellSize ?
					renderer.CellSize : 1f
				);
			}
			return Vector3.one;
		}


		#endregion




	}
}