namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	[AddComponentMenu("Jujube/Jujube Animator")]
	public class JujubeAnimator : MonoBehaviour {




		#region --- VAR ---


		// Const
		private static readonly WaitForEndOfFrame WAIT_FOR_END_OF_FRAME = new WaitForEndOfFrame();
		private const float TIME_GAP = 0.01f;
		private const int MAX_OPERATION_PER_FRAME = 16384;

		// Api
		public float Duration { get => m_Duration; set => m_Duration = value; }
		public bool PlayOnStart { get => m_PlayOnStart; set => m_PlayOnStart = value; }

		// Ser
		[SerializeField] private bool m_PlayOnStart = true;
		[SerializeField] private float m_Duration = 1f;

		// Data
		private Coroutine PlayingCor = null;


		#endregion




		#region --- MSG ---


		private void Start () {
			if (m_PlayOnStart) {
				Play();
			}
		}


		#endregion




		#region --- API ---


		public void Play () {
			if (PlayingCor != null) {
				StopCoroutine(PlayingCor);
			}
			PlayingCor = StartCoroutine(PlayLogic(true, 0f, m_Duration));
		}


		public void PlayInverse () {
			if (PlayingCor != null) {
				StopCoroutine(PlayingCor);
			}
			PlayingCor = StartCoroutine(PlayLogic(false, 0f, m_Duration));
		}


		public void Stop () {
			if (PlayingCor != null) {
				StopCoroutine(PlayingCor);
				PlayingCor = null;
			}
		}


		#endregion




		#region --- PTC ---



		protected virtual void BeforePlay () { }


		protected virtual void AfterPlay () { }


		protected virtual float GetSpreadAmount (Vector3 pos) => pos.x;


		protected virtual int SetSpreadAmount (Transform blockTF, float amount) {
			blockTF.gameObject.SetActive(amount > 0f);
			return amount > 0f ? 1 : -1;
		}


		// Transform
		protected Transform GetMapRoot () => transform;


		protected int GetChildCount (Transform target) => target.childCount;


		protected Transform GetChild (Transform target, int index) => target.GetChild(index);


		protected Vector3 GetLocalPosition (Transform target) => target.localPosition;


		#endregion




		#region --- LGC ---


		private IEnumerator PlayLogic (bool active, float startTime, float endTime) {

			BeforePlay();

			// Get Block TFs
			var blocks = new List<(Transform tf, Vector3 pos)>();
			Vector3 min = Vector3.one * float.MaxValue;
			Vector3 max = Vector3.one * float.MinValue;
			var root = GetMapRoot();
			int layerCount = GetChildCount(root);
			for (int i = 0; i < layerCount; i++) {
				var layerTF = GetChild(root, i);
				int blockCount = GetChildCount(layerTF);
				for (int j = 0; j < blockCount; j++) {
					var blockTF = GetChild(layerTF, j);
					Vector3 pos = GetLocalPosition(blockTF);
					blocks.Add((blockTF, pos));
					min = Vector3.Min(min, pos);
					max = Vector3.Max(max, pos);
				}
			}

			// Operation
			int operationLimitIndex = MAX_OPERATION_PER_FRAME;
			if (
				(min.x < max.x || Mathf.Approximately(min.x, max.x)) &&
				(min.y < max.y || Mathf.Approximately(min.y, max.y)) &&
				(min.z < max.z || Mathf.Approximately(min.z, max.z))
			) {
				// Play
				for (float time = startTime - TIME_GAP; time < endTime + TIME_GAP && blocks.Count > 0; time += Time.deltaTime) {
					float amount01 = active ?
						Remap(startTime, endTime, 0f, 1f, time) :
						Remap(startTime, endTime, 1f, 0f, time);
					for (int i = 0; i < blocks.Count; i++) {
						// Operation
						i = Operate(i, amount01, true);
						// Limit
						if (--operationLimitIndex < 0) {
							operationLimitIndex = MAX_OPERATION_PER_FRAME;
							yield return WAIT_FOR_END_OF_FRAME;
						}
					}
					yield return WAIT_FOR_END_OF_FRAME;
				}
			}

			// Last Operate
			float duration = Mathf.Max(m_Duration, 0.0001f);
			for (int i = 0; i < blocks.Count; i++) {
				i = Operate(i, active ? (endTime + TIME_GAP) / duration : (startTime - TIME_GAP) / duration, false);
			}

			// End
			AfterPlay();
			PlayingCor = null;

			// === Func ===
			int Operate (int i, float amount01, bool removeOnDone) {
				var (blockTF, pos) = blocks[i];
				float blockAmount01 = GetSpreadAmount(new Vector3(
					Remap(min.x, max.x, 0f, 1f, pos.x),
					Remap(min.y, max.y, 0f, 1f, pos.y),
					Remap(min.z, max.z, 0f, 1f, pos.z)
				));
				// Done Block
				int spreadID = SetSpreadAmount(blockTF, amount01 - blockAmount01);
				if ((active && spreadID == 1) || (!active && spreadID == -1)) {
					if (removeOnDone) {
						blocks.RemoveAt(i);
						i--;
					}
				}
				return i;
			}


		}


		private float Remap (float l, float r, float newL, float newR, float t) => l == r ? l : Mathf.LerpUnclamped(newL, newR, (t - l) / (r - l));


		#endregion




	}
}