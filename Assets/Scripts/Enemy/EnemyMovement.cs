using UnityEngine;

public class EnemyMovement : MonoBehaviour, IManagerMove
{
     public AnimationController animationController;
     private EnemyCollider enemyCollider;
     public float speed = 3;
     private Vector3 _itemGroundPosition;
     public Vector3 itemGroundPosition { get => _itemGroundPosition; set => _itemGroundPosition = value; }
     private MoveType _moveType;
     public MoveType MoveType { get => _moveType; set => _moveType = value; }

     
     private void Start() {
           enemyCollider = GetComponent<EnemyCollider>();
     }

     internal void Move()
     {
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
          Move();
     }

     public void Stop()
     {
          enemyCollider.ChangeMoveType();
     }
}
