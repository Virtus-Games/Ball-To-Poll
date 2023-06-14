using System.Collections.Generic;
using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
     [Header("Enemy")]
     private EnemyMovement enemyMovement;
     private EnemyAttack enemyAttack;

     [Header("Rays")]
     private bool _isSearch = false;
     [SerializeField] private List<RayCollider> rays;
     public bool IsSearch { get => _isSearch; set => _isSearch = value; }

     private Vector3 _itemGroundPosition;
     public Vector3 itemGroundPosition { get => _itemGroundPosition; set => _itemGroundPosition = value; }

     private void Start()
     {
          enemyMovement = GetComponent<EnemyMovement>();
          enemyAttack = GetComponent<EnemyAttack>();
     }


     private void Update() => MoveActiveAtRay();

     public void MoveActiveAtRay()
     {
          if (!_isSearch)
          {
               foreach (RayCollider collider in rays)
               {
                    if (enemyMovement.MoveType == collider.moveType)
                         collider.GetGroundIsHave();
               }
          }
     }


     public void ChangeMoveType()
     {
          if (enemyMovement.MoveType == MoveType.FORWARD)
               enemyMovement.MoveType = MoveType.BACK;
          else if (enemyMovement.MoveType == MoveType.BACK)
               enemyMovement.MoveType = MoveType.FORWARD;


     }
}
