using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : Singleton<PlayerMovement>
{

     public float speed = 5f;
     [SerializeField] private float moveDistance = 5f;
     [SerializeField] private CharacterController Cc;
     [SerializeField] private Collider bounds;


     private void Start()
     {
          moveDistance = bounds.bounds.size.x / speed;
     }

     internal void MoveCharacter(Vector3 direction)
     {
          Cc.Move(direction * moveDistance);
     }
}