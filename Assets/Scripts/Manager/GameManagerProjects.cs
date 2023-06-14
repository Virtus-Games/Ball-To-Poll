using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public enum GAMESTATE
{
    START,
    PLAY,
    VICTORY,
    DEFEAT,
     MARKET,
}

[ExecuteInEditMode]
public class GameManagerProjects : Singleton<GameManagerProjects>
{

    #region  Value Settings

    [HideInInspector]
    public static event Action<GAMESTATE> OnGameStateChanged;
    [HideInInspector]
    public GAMESTATE gameState;
    public bool isPlay;
    public static event Action<bool> OnPlayerHaveInGame;

    private int _currentLevelCoin;
    public int CurrentLevelCoin
    {
        get { return _currentLevelCoin; }
        set { _currentLevelCoin += value; }
    }


    #endregion

    public void UpdateGameState(GAMESTATE state)
    {
        gameState = state;


        Debug.Log("Here");

        switch (gameState)
        {
            case GAMESTATE.START:
                HandleStartAction();
                break;
            case GAMESTATE.PLAY:
                isPlay = true;

                break;
            case GAMESTATE.VICTORY:
                isPlay = false;
                break;
            case GAMESTATE.DEFEAT:
                isPlay = false;
                break;
        }

        OnGameStateChanged?.Invoke(state);
    }
    private void Awake()
    {
    }

     #region Update States

     private void HandleStartAction() => CurrentLevelCoin = 0;


     #endregion

     #region  Player Status Manager

     public void UpdatePlayerStatus(bool isHave) => OnPlayerHaveInGame?.Invoke(isHave);

    #endregion  Player Status Manager


}


#if UNITY_EDITOR


[CustomEditor(typeof(GameManagerProjects))]
public class GameManagerEditor : Editor
{
    public GAMESTATE state;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameManagerProjects gameManager = target as GameManagerProjects;

        EditorGUILayout.LabelField("Editor Status");

        state = (GAMESTATE)EditorGUILayout.EnumPopup("Game State", state);
        if (GUILayout.Button("Update Game State"))
        {
            gameManager.UpdateGameState(state);
        }
    }

}
#endif