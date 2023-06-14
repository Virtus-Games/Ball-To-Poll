namespace JujubeMapEditor.Core {



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class DisableAttribute : UnityEngine.PropertyAttribute { }



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class NullAlertAttribute : UnityEngine.PropertyAttribute { }



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ClampAttribute : UnityEngine.PropertyAttribute {
		public readonly float Min, Max;
		public ClampAttribute (float min, float max) {
			Min = min;
			Max = max;
		}
	}



}




#if UNITY_EDITOR
namespace JujubeMapEditor.Editor {
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEditor;
	using JujubeMapEditor.Core;



	[CustomPropertyDrawer(typeof(DisableAttribute))]
	public class Disable_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			bool oldE = GUI.enabled;
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label);
			GUI.enabled = oldE;
		}
	}



	[CustomPropertyDrawer(typeof(NullAlertAttribute))]
	public class NullAlert_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.PropertyField(position, property, label);
			if (property.objectReferenceValue == null) {
				var oldC = GUI.color;
				GUI.color = new Color(1f, 0f, 0f, 1f);
				GUI.Box(new Rect(0, position.y, position.width + position.x, position.height), GUIContent.none);
				GUI.color = oldC;
			}
		}
	}


	[CustomPropertyDrawer(typeof(ClampAttribute))]
	public class Clamp_AttributeDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			switch (property.propertyType) {
				case SerializedPropertyType.Float: {
					var cAtt = attribute as ClampAttribute;
					property.floatValue = Mathf.Clamp(property.floatValue, cAtt.Min, cAtt.Max);
					break;
				}
				case SerializedPropertyType.Integer: {
					var cAtt = attribute as ClampAttribute;
					property.intValue = Mathf.Clamp(property.intValue, (int)cAtt.Min, (int)cAtt.Max);
					break;
				}
			}
			EditorGUI.PropertyField(position, property, label);
		}
	}


}
#endif