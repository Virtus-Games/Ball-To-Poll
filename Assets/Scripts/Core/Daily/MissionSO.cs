using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create Dailys", fileName = "Dailys Missions")]
public class MissionSO : ScriptableObject
{
    public MissionClass[] missions;

    public int GetLength(){
        return missions.Length;
    }

    public MissionClass Initiliaze(int missionIndex) => missions[missionIndex];

}


[System.Serializable]
public class MissionClass
{
    public enum Rewardtype
    {
        MONEY,
        ROBOT,
    }

    public string missionName;
    public int moneyCount;
 
}


