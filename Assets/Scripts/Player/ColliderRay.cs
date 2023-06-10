using UnityEngine;

public class ColliderRay : MonoBehaviour
{
     private float raycastDistance = 10f;
     private float rayLength = 10f;
     internal bool isMoveActive;
     internal MoveType moveType;

     internal bool ControllerHit()
     {
          RaycastHit hit;

          if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
          {
               GameObject hitObject = hit.collider.gameObject;

               if (hitObject.TryGetComponent(out Ground ground))
               {
                    if (transform.root.gameObject.TryGetComponent(out ICollider collider)) collider.MoveTypeAtRay = moveType;
                    return true;
               }
               else
               {
                    SearchCollider();
                    return false;
               }
          }
          else
          {
               SearchCollider();
               return false;
          }
     }

     private void SearchCollider()
     {
          if (transform.root.gameObject.TryGetComponent(out ICollider collider))
          {
               collider.IsSearch = true;
               collider.Search();
          }
     }

     internal bool SearchingGround()
     {
          RaycastHit hit;

          if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
          {
               GameObject hitObject = hit.collider.gameObject;

               if (hitObject.TryGetComponent(out Ground ground))
                    return true;
               else
                    return false;
          }

          return false;
     }

     private void OnDrawGizmos() => Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.red);

}