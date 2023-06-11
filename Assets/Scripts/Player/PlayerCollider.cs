using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum MoveType
{
     FORWARD,
     BACK,
     RIGHT,
     LEFT
}

public class PlayerCollider : Singleton<PlayerCollider>
{
     [SerializeField] private float distanceX;
     [SerializeField] private float distanceY;
     public ColliderRay colliderRay;
     public List<ColliderRay> colliderRays;
     public float yDistance = 1;

     private void Start() => InstantColliders();

     private void OnTriggerEnter(Collider other)
     {
          if (other.gameObject.TryGetComponent(out Interactable interactable)) interactable.Started();
     }

     private void OnTriggerStay(Collider other)
     {
          // if (other.gameObject.TryGetComponent(out Interactable interactable)) interactable.Uptaded();
     }

     private void OnTriggerExit(Collider other)
     {
          if (other.gameObject.TryGetComponent(out Interactable interactable)) interactable.Endend();
     }


     private void InstantColliders()
     {
          Vector3 pos = transform.position;

          ColliderRay _LeftRaycast = Instantiate(colliderRay, (DistanceX(pos, -distanceX)), Quaternion.identity);
          _LeftRaycast.moveType = MoveType.LEFT;
          ColliderRay _RightRaycast = Instantiate(colliderRay, (DistanceX(pos, distanceX)), Quaternion.identity);
          _RightRaycast.moveType = MoveType.RIGHT;

          ColliderRay _ForwardRaycast = Instantiate(colliderRay, (DistanceY(pos, distanceY)), Quaternion.identity);
          _ForwardRaycast.moveType = MoveType.FORWARD;
          ColliderRay _BackRaycast = Instantiate(colliderRay, (DistanceY(pos, -distanceY)), Quaternion.identity);
          _BackRaycast.moveType = MoveType.BACK;

          colliderRays.Add(_BackRaycast);
          colliderRays.Add(_LeftRaycast);
          colliderRays.Add(_RightRaycast);
          colliderRays.Add(_ForwardRaycast);

          foreach (ColliderRay collider in colliderRays) collider.transform.SetParent(transform);
     }

     private Vector3 DistanceX(Vector3 pos, float distance) => new Vector3(pos.x + distance, yDistance, pos.z);
     private Vector3 DistanceY(Vector3 pos, float distance) => new Vector3(pos.x, yDistance, pos.z + distance);

     public bool IsMoveActive()
     {
          foreach (ColliderRay item in colliderRays)
               if (PlayerMovement.Instance.GetMoveType() == item.moveType)
                    if (item.ControllerHit() == false) return false;

          return true;
     }
}