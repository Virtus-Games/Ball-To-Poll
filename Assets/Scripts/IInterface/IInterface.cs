
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


public interface Interactable
{
     bool Status { get; set; }

     public void StatusController();
}

public interface IGround
{
     public Vector3 itemGroundPosition { get; set; }
}


public interface IWalkable
{

}

public interface IAnimation
{
     public string OpenButtonTrigger();
     public string CloseButtonTrigger();
}

public interface ICollider
{
     MoveType MoveTypeAtRay { get; set; }
     bool IsSearch { get; set; }
     void ChangeMoveType(MoveType moveType);
     public void Search();
}