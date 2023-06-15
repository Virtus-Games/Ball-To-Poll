
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("Win Panel")]
    public TextMeshProUGUI WinText;
    public TextMeshProUGUI WinText2x;

    [Header("In Game Panel")]
    public TextMeshProUGUI LevelIndex;

    [Header("Home Panel")]
    [SerializeField] private TextMeshProUGUI CoinCount;

    [SerializeField] private GameObject HomePanel, InGamePanel, WinPanel, LosePanel;
    [SerializeField] private GameObject NoThanksButton;


    private void OnEnable()
    {
        PanelController(HomePanel);
        GameManagerProjects.OnGameStateChanged += OnGame;
        LevelManager.OnLevelLoaded += UpdateLevel;
        GameManagerProjects.Instance.UpdateGameState(GAMESTATE.START);

        PlayerData.onDataChanged += OnDataChanged;
        PlayerData.Instance.Save();
    
    }

    private void OnDataChanged(PlayerDataContainer arg0)
    {
        CoinCount.text = PlayerData.playerData.coinCount.ToString();
    }

    private void OnDisable()
    {
        GameManagerProjects.OnGameStateChanged -= OnGame;
        LevelManager.OnLevelLoaded -= UpdateLevel;
        PlayerData.onDataChanged -= OnDataChanged;

    }

    private void UpdateLevel(bool arg0)
    {

    }


    public void PanelController(GameObject panel)
    {

        HomePanel.SetActive(false);
        InGamePanel.SetActive(false);
        WinPanel.SetActive(false);
        LosePanel.SetActive(false);

        panel.SetActive(true);
    }

    private void OnGame(GAMESTATE obj)
    {

        if (obj == GAMESTATE.START)
        {
            CoinCount.text = PlayerData.playerData.coinCount.ToString();
            ShowNoThanksButton(false, 0f);
            PanelController(HomePanel);
        }

        if (obj == GAMESTATE.PLAY)
        {
            LevelIndex.text = LevelManager.Instance.GetLevelIndex().ToString();
            PanelController(InGamePanel);
        }

        if (obj == GAMESTATE.VICTORY)
        {
            WinText.text = GameManagerProjects.Instance.CurrentLevelCoin.ToString();
            WinText2x.text = (GameManagerProjects.Instance.CurrentLevelCoin * 2).ToString();
            PanelController(WinPanel);
            ShowNoThanksButton(true, 3f);
        }

        if (obj == GAMESTATE.DEFEAT)
            PanelController(LosePanel);
    }

    IEnumerator ShowNoThanksButton(bool value, float waittime)
    {
        yield return new WaitForSeconds(waittime);
        NoThanksButton.SetActive(value);
    }



    public void SettingsButton()
    {

    }

    public void SettingsCloseButton()
    {
        if (GameManagerProjects.Instance.gameState == GAMESTATE.START)
            PanelController(HomePanel);
        else if (GameManagerProjects.Instance.gameState == GAMESTATE.PLAY)
            PanelController(InGamePanel);
    }



}
