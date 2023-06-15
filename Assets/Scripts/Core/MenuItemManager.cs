using UnityEngine;
using UnityEngine.UI;

public class MenuItemManager : MonoBehaviour
{

     // StartPanel Market Icon Component Button
     public void MarketButton()
     {
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.MARKET);
     }

     // StartPanel -> Top Panel -> Settings Icon Component Button
     public void Settings(GameObject obj)
     {
          Time.timeScale = 0;
          obj.SetActive(true);
     }


     // Victory Panel -> Next Level Compoenet Button
     public void LevelLoader()
     {
          LevelManager.Instance.SceneLoader(PlayerData.playerData.currentLevel.ToString());
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.START);
     }

     // Victory Panel
     public void VictoryPanel()
     {
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.VICTORY);
     }

     // Defeat Panel
     public void DefeatPanel()
     {
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.DEFEAT);
     }


     // Start Panel -> Remove Ads
     public void RemoveAds()
     {
          // AdsManager.Instance.RemoveAds();
     }

     public void Winx2()
     {
          // AdmonController.Instance.SetStatus(AdmobStatus.WinX2);
     }

     public void SkipLevel()
     {
          // AdmonController.Instance.SetStatus(AdmobStatus.SkinLevel);
     }



     // SettingPanel -> Close Icon Component Button
     public void GoStartPanel(GameObject obj)
     {
          obj.SetActive(false);
          Time.timeScale = 1;
     }

     // SettingPanel -> Close Icon Component Button
     public void StartPanel()
     {
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.START);
          Time.timeScale = 1;
     }


     // StartPanel Component Button
     public void PlayButton() => GameManagerProjects.Instance.UpdateGameState(GAMESTATE.PLAY);

     public void NextLevelButton()
     {
          PlayerData.playerData.coinCount += GameManagerProjects.Instance.CurrentLevelCoin;
          PlayerData.Instance.Save();
          LevelManager.Instance.SetLevelUp();

          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.START);
     }

     public void RestartLevelButton()
     {
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.START);
          LevelManager.Instance.LevelLoad();
     }

}
