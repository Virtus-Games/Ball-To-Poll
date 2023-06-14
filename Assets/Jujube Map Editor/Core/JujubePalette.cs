namespace JujubeMapEditor.Core {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;



	[CreateAssetMenu(menuName = "Jujube Palette", fileName = "New Palette", order = 204)]
	public class JujubePalette : ScriptableObject {



		// Api
		public GameObject this[int index] => index >= 0 && index < m_Prefabs.Count ? m_Prefabs[index] : null;
		public int Count => m_Prefabs.Count;
		public GameObject Failback { get => m_Failback; set => m_Failback = value; }
		public List<GameObject> Prefabs => m_Prefabs;

		// Ser
		[SerializeField] private GameObject m_Failback = null;
		[SerializeField] private List<GameObject> m_Prefabs = new List<GameObject>();



		// API
		public void Sort () => m_Prefabs.Sort((a, b) => a != null && b != null ? a.name.CompareTo(b.name) : (a == null ? 1 : -1));


		public void RemoveEmpty () {
			for (int i = 0; i < m_Prefabs.Count; i++) {
				if (m_Prefabs[i] == null) {
					m_Prefabs.RemoveAt(i);
					i--;
				}
			}
		}


		// Item
		public void DuplicateItem (int index, JujubeMap map = null) {
			if (index >= 0 && index < m_Prefabs.Count) {
				GameObject[][] prefabss = null;
				if (map != null) {
					prefabss = map.GetBlockPrefabs(this);
				}
				m_Prefabs.Insert(index, m_Prefabs[index]);
				if (prefabss != null) {
					map.SetBlockPrefabIndexs(prefabss, this);
				}
			}
		}


		public void DeleteItem (int index, JujubeMap map = null) {
			if (index >= 0 && index < m_Prefabs.Count) {
				GameObject[][] prefabss = null;
				if (map != null) {
					prefabss = map.GetBlockPrefabs(this);
				}
				m_Prefabs.RemoveAt(index);
				if (prefabss != null) {
					map.SetBlockPrefabIndexs(prefabss, this);
				}
			}
		}


		public void SwipeItem (int indexA, int indexB, JujubeMap map = null) {
			if (indexA < 0 || indexA >= m_Prefabs.Count || indexB < 0 || indexB >= m_Prefabs.Count) { return; }
			GameObject[][] prefabss = null;
			if (map != null) {
				prefabss = map.GetBlockPrefabs(this);
			}
			var temp = m_Prefabs[indexA];
			m_Prefabs[indexA] = m_Prefabs[indexB];
			m_Prefabs[indexB] = temp;
			if (prefabss != null) {
				map.SetBlockPrefabIndexs(prefabss, this);
			}
		}


		public void MoveItem (int index, int newIndex, JujubeMap map = null) {
			if (index == newIndex || index < 0 || index >= m_Prefabs.Count || newIndex < 0 || newIndex >= m_Prefabs.Count) { return; }
			GameObject[][] prefabss = null;
			if (map != null) {
				prefabss = map.GetBlockPrefabs(this);
			}
			var temp = m_Prefabs[index];
			m_Prefabs.RemoveAt(index);
			m_Prefabs.Insert(newIndex, temp);
			if (prefabss != null) {
				map.SetBlockPrefabIndexs(prefabss, this);
			}
		}


		public void AddItem (GameObject prefab) => m_Prefabs.Add(prefab);


		public void SetItemPrefab (int index, GameObject prefab) {
			if (index >= 0 && index < m_Prefabs.Count) {
				m_Prefabs[index] = prefab;
			}
		}


		public void SortByName (JujubeMap map = null) {
			if (m_Prefabs.Count == 0) { return; }
			GameObject[][] prefabss = null;
			if (map != null) {
				prefabss = map.GetBlockPrefabs(this);
			}
			m_Prefabs.Sort(
				(a, b) =>
				a != null && b != null ? a.name.CompareTo(b.name) :
				a == null ? -1 : 1
			);
			if (prefabss != null) {
				map.SetBlockPrefabIndexs(prefabss, this);
			}
		}


	}
}