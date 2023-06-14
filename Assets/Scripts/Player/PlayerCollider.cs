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
     [SerializeField] private float distanceZ;
     public float yDistance = 1.128f;
     public RayCollider colliderRay;
     public List<RayCollider> colliderRays;

     private void Start() => InstantColliders();

     private void OnTriggerEnter(Collider other)
     {
          if (other.gameObject.TryGetComponent(out Interactable interactable))
               interactable.StatusController();
     }


     public void IsMoveActive()
     {
          foreach (RayCollider item in colliderRays)
          {
               if (PlayerMovement.Instance.GetMoveType() == item.moveType)
                    item.GetGroundIsHave();
          }

     }


     private Vector3 DistanceX(Vector3 pos, float distance) => new Vector3(pos.x + distance, yDistance, pos.z);
     private Vector3 DistanceZ(Vector3 pos, float distance) => new Vector3(pos.x, yDistance, pos.z + distance);




     private void InstantColliders()
     {
          Vector3 pos = transform.position;

          RayCollider _LeftRaycast = Instantiate(colliderRay, (DistanceX(pos, -distanceX)), Quaternion.identity);
          _LeftRaycast.moveType = MoveType.LEFT;
          RayCollider _RightRaycast = Instantiate(colliderRay, (DistanceX(pos, distanceX)), Quaternion.identity);
          _RightRaycast.moveType = MoveType.RIGHT;

          RayCollider _ForwardRaycast = Instantiate(colliderRay, (DistanceZ(pos, distanceZ)), Quaternion.identity);
          _ForwardRaycast.moveType = MoveType.FORWARD;

          RayCollider _BackRaycast = Instantiate(colliderRay, (DistanceZ(pos, -distanceZ)), Quaternion.identity);
          _BackRaycast.moveType = MoveType.BACK;

          colliderRays.Add(_BackRaycast);
          colliderRays.Add(_LeftRaycast);
          colliderRays.Add(_RightRaycast);
          colliderRays.Add(_ForwardRaycast);

          foreach (RayCollider collider in colliderRays) collider.transform.SetParent(transform);
     }
}