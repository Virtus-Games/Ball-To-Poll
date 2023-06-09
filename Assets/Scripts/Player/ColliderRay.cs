using UnityEngine;

public class ColliderRay : MonoBehaviour
{
     private float raycastDistance = 10f;
     private float rayLength = 10f;
     internal bool isMoveActive;

     private MoveType moveType;
     public MoveType Movetype
     {
          get { return moveType; }
          set { moveType = value; }
     }
     


     private void OnDrawGizmos()
     {


          Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.red);
     }

     internal bool ControllerHit()
     {
          RaycastHit hit;

          if (Physics.Raycast(transform.position, Vector3.down, out hit, raycastDistance))
          {
               GameObject hitObject = hit.collider.gameObject;

               
               if (hitObject != null) return true;
               else return false;

          }
          else return false;
     }
}