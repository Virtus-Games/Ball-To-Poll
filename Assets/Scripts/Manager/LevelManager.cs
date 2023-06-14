using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
     public static event UnityAction<bool> OnLevelLoaded;
     private const string _level = "Level ";
     private Scene _lastLoadedScene;
     private int currentLevel;
     public GameObject LoadingBar;

     public bool isNOLevel = false;

     private void Awake()
     {
          //AdmobManager.Instance.InitiliazedAds();
          if (isNOLevel) return;
          LevelLoad();
     }

     internal int GetLevelIndex()
     {
          return PlayerData.playerData.currentLevel;
     }

     internal void LevelLoad()
     {
          currentLevel = PlayerData.playerData.currentLevel;
          LoadinigBarControl(true);

          SceneLoader(currentLevel.ToString());
     }

     private void LoadinigBarControl(bool val)
     {
          if (LoadingBar != null)
               LoadingBar.SetActive(val);
     }

     public void SetLevelUp()
     {
          currentLevel++;

          if (currentLevel >= SceneManager.sceneCountInBuildSettings)
               currentLevel = 1;

          PlayerData.playerData.currentLevel = currentLevel;
          PlayerData.Instance.Save();
          SceneLoader(currentLevel.ToString());
     }

     public void SceneLoader(string name)
     {
          LoadinigBarControl(true);

          StartCoroutine(SceneController(_level + name));
     }

     IEnumerator SceneController(string sceneName)
     {
          OnLevelLoaded?.Invoke(false);

          if (_lastLoadedScene.IsValid())
          {
               SceneManager.UnloadSceneAsync(_lastLoadedScene);
               bool isUnloadScene = false;
               while (!isUnloadScene)
               {
                    isUnloadScene = !_lastLoadedScene.IsValid();
                    yield return new WaitForEndOfFrame();

               }
          }

          SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

          bool isSceneLoaded = false;

          while (!isSceneLoaded)
          {
               _lastLoadedScene = SceneManager.GetSceneByName(sceneName);
               isSceneLoaded = _lastLoadedScene != null && _lastLoadedScene.isLoaded;

               yield return new WaitForEndOfFrame();
          }

          OnLevelLoaded?.Invoke(true);

     }

     internal void RestartLevel()
     {
          LevelLoad();
     }
}




#if UNITY_EDITOR


[CustomEditor(typeof(LevelManager))]
public class LevelManagerCustom : Editor
{
     public override void OnInspectorGUI()
     {
          base.OnInspectorGUI();

          if (GUILayout.Button("Next Level"))
          {
               UIManager.Instance.NextLevelButton();
          }
     }
}
#endif