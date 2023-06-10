using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Singleton<PlayerMovement>
{

     public float speed = 5f;
     [SerializeField] private float moveDistance = 5f;
     [SerializeField] private Rigidbody Rb;
     [SerializeField] private Collider bounds;

     private MoveType _moveType;

     internal void SetMoveType(MoveType moveType) => _moveType = moveType;
     internal MoveType GetMoveType() => _moveType;


     internal Bounds GetBounds() => bounds.bounds;


     private void Start()
     {
          moveDistance = bounds.bounds.size.x / speed;
     }

     internal void MoveCharacter(Vector3 direction)
     {
          if (PlayerCollider.Instance.IsMoveActive())
               transform.Translate(direction * moveDistance);
     }
}