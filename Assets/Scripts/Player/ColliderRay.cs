using UnityEngine;


public abstract class ARaycastManager : MonoBehaviour
{
     public float raycastDistance = 10f;
     internal GameObject GetHitObject()
     {
          RaycastHit hit;

          if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
          {
               GameObject hitObject = hit.collider.gameObject;
               if (hitObject != null) return hitObject;
               else return null;
          }
          else return null;
     }
}


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
          if (transform.root.gameObject.TryGetComponent(out ICollider collider))
               collider.MoveTypeAtRay = moveType;
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

     private void OnDrawGizmos() => Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.red);

}