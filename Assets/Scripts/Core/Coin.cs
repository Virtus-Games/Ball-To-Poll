using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
     private bool IsPlay;
     public float jumpVectorY = 5;
     public float jumpForce = 5;
     public int numbs = 1;
     public float timeDuration = 2;
     private bool justOne;

     private void OnTriggerEnter(Collider other)
     {
          if (other.GetComponent<PlayerCollider>() != null && !justOne)
          {
               Star(GetPos());
               justOne = true;
          }

     }

     private Vector3 GetPos()
     {
          return new Vector3(transform.position.x, jumpVectorY, transform.position.z);
     }

     private void Star(Vector3 pos)
     {
          transform.DOJump(pos, jumpForce, numbs, timeDuration).OnUpdate(() =>
          {
               if (transform.position.y == jumpVectorY)
                    IsPlay = true;
          });
     }


     void Update()
     {
          Play();

     }
     private void Play()
     {
          if (IsPlay) TranslatetoObJToCanvasPoint();
     }

     public void TranslatetoObJToCanvasPoint()
     {
          IsPlay = false;

          Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);

          CoinTranslateManager.Instance.TransformMoney(pos);
          Destroy(gameObject, 0.3f);
     }
}
