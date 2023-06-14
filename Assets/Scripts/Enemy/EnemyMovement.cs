using UnityEngine;
using DG.Tweening;



public class EnemyMovement : MonoBehaviour
{
     public AnimationController animationController;
     public float speed = 3;

     private void Start()
     {
     }

     internal void Move(MoveType type)
     {
          switch (type)
          {
               case MoveType.FORWARD:
               
                    transform.Translate(Vector3.forward * speed * Time.deltaTime);
                    break;
               case MoveType.BACK:
                    transform.Translate(Vector3.back * speed * Time.deltaTime);
                    break;
               case MoveType.LEFT:
                    transform.Translate(Vector3.left * speed * Time.deltaTime);
                    break;
               case MoveType.RIGHT:
                    transform.Translate(Vector3.right * speed * Time.deltaTime);
                    break;
          }
     }
}