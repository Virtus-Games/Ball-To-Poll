using UnityEngine;

public class ColliderRay : MonoBehaviour
{
     private float raycastDistance = 10f;
     private float rayLength = 10f;
     internal bool isMoveActive;
     public MoveType moveType;

     internal bool ControllerHit()
     {
          if (GetHitObject() != null && GetHitObject().TryGetComponent(out Ground ground))
          {
               if (transform.root.gameObject.TryGetComponent(out ICollider collider)) collider.MoveTypeAtRay = moveType;
               return true;
          }
          else
          {
               if (transform.root.gameObject.TryGetComponent(out ICollider collider))
               {
                    collider.IsSearch = true;
                    collider.Search();
               }
               return false;
          }
     }

     private GameObject GetHitObject()
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



     internal bool SearchingGround()
     {
          if (GetHitObject() != null && GetHitObject().TryGetComponent(out Ground ground))
               return true;
          else
               return false;
     }
     internal GameObject SearchPlayer()
     {
          if (GetHitObject() != null && GetHitObject().TryGetComponent(out PlayerCollider playerCollider))
               return GetHitObject();
          else
               return null;
     }

     private void OnDrawGizmos() => Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.red);

}