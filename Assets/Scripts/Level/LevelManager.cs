using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelData levelData;

    [SerializeField] private Transform levelParentTransform;

    internal void SaveLevel()
    {
        
    }
    
    
    internal void NextLevel()
    {
        
    }
    
   

    private void Start()
    {
       // PlaceObjects();
    }

    private void PlaceObjects()
    {
        for (int i = 0; i < levelData.levelObject.Count; i++)
        {
            GameObject obj = Instantiate(levelData.levelObject[i]);
            obj.transform.SetParent(levelParentTransform);
        }
    }
}

[CustomEditor(typeof(LevelManager))]
public class MyScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Save Level"))
        {
            LevelManager myScript = (LevelManager)target;
            myScript.SaveLevel();
        }
        
        if (GUILayout.Button("Next Level"))
        {
            LevelManager myScript = (LevelManager)target;
            myScript.NextLevel();
        }
    }
}