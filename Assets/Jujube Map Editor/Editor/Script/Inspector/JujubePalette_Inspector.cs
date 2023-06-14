namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using global::JujubeMapEditor.Core;



	[CustomEditor(typeof(JujubePalette))]
	public class JujubePalette_Inspector : MoenenInspector {




		SerializedProperty Prop_Prefabs = null;


		private void OnEnable () {
			Prop_Prefabs = serializedObject.FindProperty("m_Prefabs");
		}


		public override void OnInspectorGUI () {

			Space(4);

			// Drag to Add Prefab
			serializedObject.Update();
			DragGUI<GameObject>(GUIRect(0, 48), "Drag <color=#3399cc>Prefab</color> or <color=#3399cc>Folder</color> here to add them into this palette", ".prefab", (gObj) => {
				var type = PrefabUtility.GetPrefabAssetType(gObj);
				if (type == PrefabAssetType.Regular || type == PrefabAssetType.Model) {
					Prop_Prefabs.InsertArrayElementAtIndex(Prop_Prefabs.arraySize);
					Prop_Prefabs.GetArrayElementAtIndex(Prop_Prefabs.arraySize - 1).objectReferenceValue = gObj;
				}
				return false;
			});
			serializedObject.ApplyModifiedProperties();
			Space(4);

			// Default
			base.OnInspectorGUI();
		}


	}
}