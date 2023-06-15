using System.Collections.Generic;
using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
     [Header("Enemy")]
     private EnemyMovement enemyMovement;
     private EnemyAttack enemyAttack;

     [Header("Rays")]
     [SerializeField] private List<RayCollider> rays;
     private Vector3 _itemGroundPosition;
     public Vector3 itemGroundPosition { get => _itemGroundPosition; set => _itemGroundPosition = value; }
     public bool Stop { get; internal set; }

     private void Start()
     {
          enemyMovement = GetComponent<EnemyMovement>();
          enemyAttack = GetComponent<EnemyAttack>();
     }
     

     private void Update() => MoveActiveAtRay();

     public void MoveActiveAtRay()
     {
          if (!Stop)
          {
               foreach (RayCollider collider in rays)
                    if (enemyMovement.MoveType == collider.moveType)
                         collider.GetGroundIsHave();
          }
     }


     public void ChangeMoveType()
     {
          if (enemyMovement.MoveType == MoveType.FORWARD) enemyMovement.MoveType = MoveType.BACK;
          else if (enemyMovement.MoveType == MoveType.BACK) enemyMovement.MoveType = MoveType.FORWARD;
     }
}
