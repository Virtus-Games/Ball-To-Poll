using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Level 1", menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{
    public List<GameObject> levelObject;
}
