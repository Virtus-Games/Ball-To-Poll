using UnityEngine;
using DG.Tweening;

public class FollowPlayer : Singleton<FollowPlayer>
{
     public float OffsetZ;
     public Vector3 Offset;
     public float Speed;
     public Transform Player;

     internal void SetPlayer() => Player = PlayerMovement.Instance.transform;

     private void LateUpdate()
     {
          if (Player == null) return;

          Vector3 offset = (Player.position - (transform.position + Offset));
          float distance = Vector3.Distance(Player.position, transform.position);

          if (distance > OffsetZ)
          {
               transform.DOMoveZ(offset.z, Speed);
          }
     }
}
