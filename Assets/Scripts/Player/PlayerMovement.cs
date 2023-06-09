using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Singleton<PlayerMovement>
{

     public float speed = 5f;
     [SerializeField] private float moveDistance = 5f;
     [SerializeField] private CharacterController Cc;
     [SerializeField] private Collider bounds;

     private MoveType _moveType;

     internal void SetMoveType(MoveType moveType){
          _moveType = moveType;
     }
     internal MoveType GetMoveType(){
          return _moveType;
     }


     internal Bounds GetBounds()
     {
          return bounds.bounds;
     }


     private void Start()
     {
          moveDistance = bounds.bounds.size.x / speed;
     }

     internal void MoveCharacter(Vector3 direction)
     {
          if (PlayerCollider.Instance.IsMoveActive())
               Cc.Move(direction * moveDistance);
     }
}