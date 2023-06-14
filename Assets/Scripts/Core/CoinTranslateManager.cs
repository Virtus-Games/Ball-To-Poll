using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using GDTools.ObjectPooling;

public class CoinTranslateManager : Singleton<CoinTranslateManager>
{
    [Header("Settings")]
    public float distance = 2f;
    public float Speed = 2f;
    private int countCount = 0;
    
    [Header("Parents")]
    public Transform coinTransformPointParent;
    public Transform CoinPrefabUIParent;
    public Transform CoinPrefabParent;

    [Header("Pool Objects")]
    public Pool CoinPrefabUIPool;
    public Pool CoinPrefabPool;

    [Header("Events On Die")]
    public static UnityAction<bool> onDie;


    private void OnDisable() => GameManagerProjects.OnGameStateChanged -= OnGameStateChanged;
    private void OnEnable() => GameManagerProjects.OnGameStateChanged += OnGameStateChanged;


    private void OnGameStateChanged(GAMESTATE obj)
    {

    }

    public void TransformMoney(Vector2 pos) => StartCoroutine(InstantceAndTranslate(pos));


    public PoolObject GetPoolObject(Vector3 position) => CoinPrefabUIPool.InstantiateObject(position, CoinPrefabUIParent);

    IEnumerator InstantceAndTranslate(Vector3 pos)
    {

        GameObject objx = GetPoolObject(pos).gameObject;

        while (Vector3.Distance(objx.transform.position, coinTransformPointParent.transform.position) > distance)
        {
            objx.transform.position = Vector3.Lerp(objx.transform.position, coinTransformPointParent.transform.position, Speed * Time.deltaTime);
            yield return null;
        }


        CoinPrefabUIPool.DestroyObject(objx.GetComponent<PoolObject>(), 0f);
    }

    public void InstantAtZombiePoint(Vector3 pos) => CoinPrefabPool.InstantiateObject(pos, CoinPrefabParent);
}
