using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class CoinTranslateManager : Singleton<CoinTranslateManager>
{
     [Header("Settings")]
     public float distance = 2f;
     public float Duration = 2f;
     private int countCount = 0;

     [Header("Parents")]
     public Transform coinTransformPointParent;
     public Transform CoinPrefabUIParent;
     public Transform CoinPrefabParent;

     [Header("Pool Objects")]
     public GameObject StarUIElenments;
     public TextMeshProUGUI coinElements;

     [Header("Events On Die")]
     private List<GameObject> elements = new List<GameObject>();
     int count = 0;



     private void OnDisable() => GameManagerProjects.OnGameStateChanged -= OnGameStateChanged;
     private void OnEnable() => GameManagerProjects.OnGameStateChanged += OnGameStateChanged;


     private void OnGameStateChanged(GAMESTATE obj)
     {
          if (obj == GAMESTATE.START)
          {
               for (int i = 0; i < elements.Count; i++)
                    Destroy(elements[i]);

               coinElements.SetText("0");
          }
     }

     public void TransformMoney(Vector2 pos) => InstantceAndTranslate(pos);


     public GameObject GetPoolObject(Vector3 position) => Instantiate(StarUIElenments, CoinPrefabUIParent);


     void InstantceAndTranslate(Vector3 pos)
     {
          GameObject objx = GetPoolObject(pos).gameObject;

          objx.transform.DOMove(coinTransformPointParent.transform.position, Duration).OnComplete(() =>
          {
               elements.Add(objx);
               GameManagerProjects.Instance.CurrentLevelCoin += 25;
               coinElements.SetText((GameManagerProjects.Instance.CurrentLevelCoin).ToString());
          });
     }
}
