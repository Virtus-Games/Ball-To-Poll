using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IManagerMove
{
     public void Run();
     public void Stop();

     // public Vector3 itemGroundPosition { get; set; }
     // public void Attack();
}

public class RayCollider : ARaycastManager
{
     public MoveType moveType;

     private IManagerMove move;

     private void Start()
     {
          move = transform.parent.GetComponent<IManagerMove>();
     }

     public void GetGroundIsHave()
     {

          if (GetHitObject() != null && GetHitObject().TryGetComponent(out IGround ground))
          {
               EnemyMoveSettings();
               PlayerMoveSettings(GetHitObject().transform);
          }
     }

     private void PlayerMoveSettings(Transform ground)
     {
          if (transform.parent.gameObject.TryGetComponent(out IGround iground))
               iground.itemGroundPosition = ground.transform.position;
     }

     private void EnemyMoveSettings()
     {

          if (GetHitObject() != null && GetHitObject().TryGetComponent(out PlayerCollider playerCollider))
          {
               // Attack Player
          }

          if (transform.root.gameObject.TryGetComponent(out ICollider collider))
               collider.MoveTypeAtRay = moveType;
     }
}
