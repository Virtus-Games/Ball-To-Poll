using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using System;

public class Mission : MonoBehaviour
{
    private bool _isMissionTimeAvailable = false;
    internal bool IsMissionTimeAvailable
    {
        get { return _isMissionTimeAvailable; }
        set
        {
            _isMissionTimeAvailable = value;
        }
    }

    [Header("Texts")]
    public TextMeshProUGUI MissionNameText;
    public TextMeshProUGUI MoneyCountText;
    public string MissionNameBeginText = "Next Mission in: ";

    [Header("Buttons GameObjects")]
    public GameObject ClaimButton;
    public GameObject NotCompareButton;
    private MissionClass _missionSO;

    [Header("Buttons")]
    public Button onClaim;

    public void Get(int index)
    {


        _missionSO = DailyManager.Instance.GetMissionClass(index);
        Set();
    }


    public void Set()
    {
        MissionNameText.SetText(MissionNameBeginText + _missionSO.missionName);
        MoneyCountText.SetText(_missionSO.moneyCount.ToString());
    }

    internal void Active(bool value)
    {
        IsMissionTimeAvailable = value;

        if (IsMissionTimeAvailable)
        {
            ClaimButton.SetActive(true);
            NotCompareButton.SetActive(false);
        }
        else
        {
            NotCompareButton.SetActive(true);
            ClaimButton.SetActive(false);
        }
    }

    public void OnClaim()
    {
        if (IsMissionTimeAvailable)
        {
            DailyManager.Instance.OnClaim();
            IsMissionTimeAvailable = false;
            Active(_isMissionTimeAvailable);
        }
    }

    internal void AddTime(string time)
    {
       MissionNameText.SetText(_missionSO.missionName + " " + time);
    }
}
