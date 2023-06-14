using UnityEngine;

public interface IManagerMove
{
     public void Run();
     public void Stop();
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
          if (GetHitObject() != null && GetHitObject().TryGetComponent(out Obstackle obstackle))
          {
               move.Stop();
               return;
          }

          if (GetHitObject() != null && GetHitObject().TryGetComponent(out IGround ground))
          {
               EnemyMoveSettings();
               PlayerMoveSettings();

               move.itemGroundPosition = GetHitObject().transform.position;
               move.MoveType = moveType;

               move.Run();
          }
          else
          {
               move.Stop();
          }
     }



     private void PlayerMoveSettings()
     {

     }

     private void EnemyMoveSettings()
     {




          if (GetHitObject() != null && GetHitObject().TryGetComponent(out PlayerCollider playerCollider))
          {

               // Attack Player
          }

     }
}
