using System;
using UnityEditor;
using UnityEngine;

public class DailyManager : Singleton<DailyManager>
{
    private const string _rewardTimeKey = "Reward_Claim_Time";
    private const string nextRewardKey = "Next_Reward_Index";
    [Header("MissionSO Scriptable")]
    public MissionSO MissionSO;
    public int nextRewardIndex = 0;

    [Header("Mission Transform")]
    public GameObject missionPrefab;
    public Transform missionTransformParent;

    [Header("Daily Manager Time Settings")]
    // public double nextRewardTime = 86400;
    public double nextRewardTime = 10;
    private Mission[] missionClasses = new Mission[3];
    private void Awake() => Initialize();

    public void Initialize()
    {

        if (string.IsNullOrEmpty(PlayerPrefs.GetString(_rewardTimeKey)))
            PlayerPrefs.SetString(_rewardTimeKey, DateTime.Now.ToString());


        nextRewardIndex = PlayerPrefs.GetInt(nextRewardKey, 0);

        if (nextRewardIndex > MissionSO.missions.Length)
        {
            nextRewardIndex = 0;
            PlayerPrefs.SetInt(nextRewardKey, nextRewardIndex);
        }


        for (int i = 0; i < 3; i++)
        {
            GameObject mission = Instantiate(missionPrefab, missionTransformParent);
            mission.GetComponent<Mission>().Get(i);
            missionClasses[i] = mission.GetComponent<Mission>();
        }

        CheckRewards();
    }

    private void OnEnable() => GameManagerProjects.OnGameStateChanged += OnGameData;

    private void OnDisable() => GameManagerProjects.OnGameStateChanged -= OnGameData;

    private void OnGameData(GAMESTATE obj)
    {
        if (obj == GAMESTATE.PLAY) CheckRewards();
    }

    internal void CheckRewards()
    {

        DateTime currentTime = DateTime.Now;
        DateTime rewardClaimDate = DateTime.Parse(PlayerPrefs.GetString(_rewardTimeKey, currentTime.ToString()));

        double elapsedTime = (currentTime - rewardClaimDate).TotalSeconds;
        double hours = (currentTime - rewardClaimDate).TotalHours;
        double minute = (currentTime - rewardClaimDate).TotalMinutes;
        double seconds = (currentTime - rewardClaimDate).TotalMilliseconds;
        
        string time = (currentTime - rewardClaimDate).ToString();

        string time2 = string.Format("{0}", hours) + ":" + string.Format("{0}", minute);
        String.Format("{0:T}", time);

        if (elapsedTime > nextRewardTime)
        {
            missionClasses[nextRewardIndex].Active(true);
            missionClasses[nextRewardIndex].AddTime(time2);

        }
        else missionClasses[nextRewardIndex].Active(false);


    }

    public MissionClass GetMissionClass(int index) => MissionSO.Initiliaze(index);


    public void OnClaim()
    {

        Debug.Log("Reward Class OnClaim" + nextRewardIndex);

        PlayerPrefs.SetString(_rewardTimeKey, DateTime.Now.ToString());
        nextRewardIndex++;

        if (nextRewardIndex > missionClasses.Length) nextRewardIndex = 0;

        PlayerPrefs.SetInt(nextRewardKey, nextRewardIndex);

    }
}


[CustomEditor(typeof(DailyManager))]
public class DailyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DailyManager daily = target as DailyManager;
        if (GUILayout.Button("Check Daily"))
            daily.CheckRewards();

        base.OnInspectorGUI();

    }
}