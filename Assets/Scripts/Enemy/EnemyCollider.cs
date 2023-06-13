using System.Collections.Generic;
using UnityEngine;



public class EnemyCollider : MonoBehaviour, ICollider, IManagerMove
{
     [Header("Enemy")]
     private EnemyMovement enemyMovement;
     private EnemyAttack enemyAttack;
     [Header("Rays")]
     [SerializeField] private List<ColliderRay> rays;
     private bool _isSearch = false;
     public bool IsSearch { get => _isSearch; set => _isSearch = value; }
     private MoveType _moveTypeAtRay = MoveType.FORWARD;
     public MoveType MoveTypeAtRay { get => _moveTypeAtRay; set => _moveTypeAtRay = value; }

     private void Start()
     {
          enemyMovement = GetComponent<EnemyMovement>();
          enemyAttack = GetComponent<EnemyAttack>();
     }

     public void MoveActiveAtRay()
     {
          if (!_isSearch)
          {
               foreach (ColliderRay item in rays)
               {
                    if (item.moveType == _moveTypeAtRay && item.ControllerHit())
                         enemyMovement.Move(item.moveType);

               }
          }
     }

     public void Search()
     {
          foreach (ColliderRay item in rays)
          {
               if (item.moveType == MoveType.FORWARD && item.SearchPlayer() != null)
               {
                    _isSearch = true;
                    enemyAttack.Attack(item.SearchPlayer());
                    break;
               }
               else
               {
                    IsSearch = item.SearchingGround();
                    if (IsSearch)
                    {
                         MoveTypeAtRay = item.moveType;
                         IsSearch = false;
                         break;
                    }
               }
          }
     }

     private void onCollisionEnter(Collider other) {
          
     }


     private void Update()
     {
          MoveActiveAtRay();
     }
     public bool RayControl(ColliderRay item)
     {
          if (item.SearchingGround())
          {
               _isSearch = true;
               IsSearch = item.SearchingGround();
               if (IsSearch)
               {
                    MoveTypeAtRay = item.moveType;
                    IsSearch = false;
                    return true;
               }
          }

          return false;
     }

     public void ChangeMoveType(MoveType moveType)
     {
          if (moveType == MoveType.FORWARD)
               MoveTypeAtRay = MoveType.BACK;
          else if (moveType == MoveType.BACK)
               MoveTypeAtRay = MoveType.FORWARD;

     }

     public void Run()
     {
     }

     public void Stop()
     {
     }


     // public void LeftOrRight()
     // {
     //      if (_moveTypeAtRay == MoveType.RIGHT || _moveTypeAtRay == MoveType.LEFT)
     //           return;

     //      foreach (ColliderRay item in rays)
     //      {
     //           if (item.moveType == MoveType.RIGHT && _moveTypeAtRay != MoveType.RIGHT)
     //           {
     //                if (RayControl(item)) { }
     //                break;
     //           }
     //           else if (item.moveType == MoveType.LEFT && _moveTypeAtRay != MoveType.LEFT)
     //           {
     //                if (RayControl(item)) { }
     //                break;
     //           }
     //      }
     // }
}
