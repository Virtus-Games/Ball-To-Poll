using UnityEngine;





public class ColliderRay : ARaycastManager
{
     private float rayLength = 10f;
     internal bool isMoveActive;
     public MoveType moveType;

     internal bool ControllerHit()
     {
          if (GetHitObject() != null && GetHitObject().TryGetComponent<IWalkable>(out IWalkable a))
          {
               ControllerHit(GetHitObject().transform);
               return true;
          }

          if (GetHitObject() != null && (GetHitObject().TryGetComponent(out Ground ground)))
          {
               if ((ground != null && ground.isMoveActive))
               {
                    ControllerHit(GetHitObject().transform);
                    return true;
               }
               else
                    return false;
          }
          else
          {
               if (transform.parent.gameObject.TryGetComponent(out ICollider collider))
                    collider.ChangeMoveType(moveType);
               return false;
          }
     }

     private void ControllerHit(Transform ground)
     {
          // Enemy
          if (transform.root.gameObject.TryGetComponent(out IManagerMove managerMove)){
               
          }
          // Player
          if (transform.parent.gameObject.TryGetComponent(out IGround iground))
               iground.itemGroundPosition = ground.transform.position;

     }





     internal bool SearchingGround()
     {
          if (GetHitObject() != null && GetHitObject().TryGetComponent(out Ground ground)) return true;
          else return false;
     }

     internal GameObject SearchPlayer()
     {
          if (GetHitObject() != null && GetHitObject().TryGetComponent(out PlayerCollider playerCollider)) return GetHitObject();
          else return null;
     }


}