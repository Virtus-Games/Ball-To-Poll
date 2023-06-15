using UnityEngine;

public interface IManagerMove
{
     public void Run();
     public void Stop();
     public void StopAndRotate();
     public void ChangeMoveType();
     public Vector3 itemGroundPosition { get; set; }
     public MoveType MoveType { get; set; }

}

public class RayCollider : ARaycastManager
{
     public MoveType moveType;
     private IManagerMove move;

     private void Start()
     {
          move = transform.parent.GetComponent<IManagerMove>();
     }

     public void GetGroundIsHave()
     {

          EnemyMoveSettings();
          PlayerMoveSettings();

          if (GetHitObject() != null && GetHitObject().TryGetComponent(out Obstackle obstackle) && move.MoveType == MoveType.FORWARD)
          {
               move.StopAndRotate();
               return;
          }

          if (GetHitObject() != null && GetHitObject().TryGetComponent(out IGround ground))
          {
               move.itemGroundPosition = GetHitObject().transform.position;
               move.MoveType = moveType;
               move.Run();
          }
          else
          {
               if (move.MoveType == MoveType.FORWARD) move.StopAndRotate();
               else
               {
                    move.ChangeMoveType();
                    move.Stop();
               }
          }
     }



     private void PlayerMoveSettings()
     {

     }

     private void EnemyMoveSettings()
     {

          if (GetHitObject() != null && GetHitObject().TryGetComponent(out PlayerCollider playerCollider))
          {
               move.Stop();
               
               if (transform.parent.TryGetComponent(out EnemyAttack enemyAttack) && moveType == MoveType.FORWARD)
               {
                    enemyAttack.Attack(playerCollider.gameObject);
               }
               // Attack Player
          }

     }
}
