using UnityEngine;
using DG.Tweening;

public class EnemyMovement : MonoBehaviour, IManagerMove
{
     public float Duration = 2f;
     public float speed = 3;

     public AnimationController animationController;
     private EnemyCollider enemyCollider;

     private Vector3 _itemGroundPosition;
     public Vector3 itemGroundPosition { get => _itemGroundPosition; set => _itemGroundPosition = value; }

     private MoveType _moveType;
     public MoveType MoveType { get => _moveType; set => _moveType = value; }


     private void Start()
     {
          enemyCollider = GetComponent<EnemyCollider>();
     }

     internal void Move()
     {
          if (!GameManagerProjects.Instance.isPlay) return;

          switch (_moveType)
          {
               case MoveType.FORWARD:
                    transform.Translate(Vector3.forward * speed * Time.deltaTime);
                    break;
               case MoveType.BACK:
                    transform.Translate(Vector3.back * speed * Time.deltaTime);
                    break;
          }
     }

     public void Run()
     {
          if (IsCharackter()) animationController.Move();

          Move();
     }

     public void Stop()
     {
          if (IsCharackter()) animationController.Idle();
     }

     private bool IsCharackter()
     {
          if (animationController != null) return true;
          else return false;
     }

     public void StopAndRotate()
     {
          if (IsCharackter())
          {
               animationController.Idle();
          }

          enemyCollider.Stop = true;

          Vector3 myRot = transform.rotation.eulerAngles;
          myRot.y += 180;

          transform.DORotate(myRot, Duration, RotateMode.Fast).OnComplete(() =>
          {
               enemyCollider.Stop = false;
               Stop();
          });

     }

     public void ChangeMoveType()
     {
          enemyCollider.ChangeMoveType();
     }
}
