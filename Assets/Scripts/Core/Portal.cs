using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
     public Vector3 originalPosition;
     public Portal otherPortal;
     public bool IsConnected = false;

     private void Start() => IsConnected = false;

     private void OnTriggerEnter(Collider other)
     {
          if (other.TryGetComponent(out PlayerMovement player) && !IsConnected)
          {

               IsConnected = true;
               otherPortal.IsConnected = true;

               Debug.Log("Here");

               


          }
     }

     private void OnTriggerExit(Collider other)
     {
          if (other.TryGetComponent(out PlayerMovement player) && IsConnected)
          {
          }
     }
}
