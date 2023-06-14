using UnityEngine;
using DG.Tweening;
using GDTools.ObjectPooling;

public class Coin : MonoBehaviour
{
    private bool IsPlay;
    public float jumpVectorY = 5;
    public float jumpForce = 5;
    public int numbs = 1;
    public float timeDuration = 2;
    public float timeWaiting = 3;
    private float _time = 0;

    PoolObject poolObject;

    private void Start()
    {
        _time = timeWaiting;
        poolObject = GetComponent<PoolObject>();
        Vector3 pos = new Vector3(transform.position.x, jumpVectorY, transform.position.z);
        Sequence ence = transform.DOJump(pos, jumpForce, numbs, timeDuration).OnComplete(() =>
        {
            IsPlay = true;
        });
    }

    void Update() => Play();

    private void Play()
    {
        if (IsPlay)
        {
            _time -= 0.1f;

            if (_time <= 0) TranslatetoObJToCanvasPoint();

        }
    }

    public void TranslatetoObJToCanvasPoint()
    {
        _time = timeWaiting;
        transform.position = Camera.main.WorldToScreenPoint(transform.position);
        IsPlay = false;
        CoinTranslateManager.Instance.TransformMoney(transform.position);
        CoinTranslateManager.Instance.CoinPrefabPool.DestroyObject(poolObject, 0.1f);
    }
}
