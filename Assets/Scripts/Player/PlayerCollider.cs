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



     private void Start()
     {
          Vector3 pos = transform.position;

          ColliderRay _LeftRaycast = Instantiate(colliderRay, (DistanceX(pos, -distanceX)), Quaternion.identity);
          _LeftRaycast.Movetype = MoveType.LEFT;
          ColliderRay _RightRaycast = Instantiate(colliderRay, (DistanceX(pos, distanceX)), Quaternion.identity);
          _RightRaycast.Movetype = MoveType.RIGHT;

          ColliderRay _ForwardRaycast = Instantiate(colliderRay, (DistanceY(pos, distanceY)), Quaternion.identity);
          _ForwardRaycast.Movetype = MoveType.FORWARD;
          ColliderRay _BackRaycast = Instantiate(colliderRay, (DistanceY(pos, -distanceY)), Quaternion.identity);
          _BackRaycast.Movetype = MoveType.BACK;

          colliderRays.Add(_BackRaycast);
          colliderRays.Add(_LeftRaycast);
          colliderRays.Add(_RightRaycast);
          colliderRays.Add(_ForwardRaycast);

          foreach (ColliderRay collider in colliderRays)
          {
               collider.transform.SetParent(transform);
          }

     }

     private Vector3 DistanceX(Vector3 pos, float distance) => new Vector3(pos.x + distance, pos.y, pos.z);
     private Vector3 DistanceY(Vector3 pos, float distance) => new Vector3(pos.x, pos.y, pos.z + distance);


     public bool IsMoveActive()
     {

          foreach (ColliderRay item in colliderRays)
          {
               if (PlayerMovement.Instance.GetMoveType() == item.Movetype)
                    if (item.ControllerHit() == false) return false;
          }

          return true;
     }
}