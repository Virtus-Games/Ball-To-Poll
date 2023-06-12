using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

public class DoorState : ARaycastManager, Interactable
{
     [SerializeField] private bool openDoor = false;
     public float duration;
     public float YValue = 90;
     public bool Status { get => openDoor; set => openDoor = value; }
     public Transform doorObje;


     public void Endend()
     {
          StartCoroutine(RotationMovement(-YValue));

     }

     private void Start()
     {
          ground().isMoveActive = openDoor;
     }


     public void Started()
     {
          if (ground().isMoveActive)
               StartCoroutine(RotationMovement(YValue));
     }


     internal Ground ground()
     {
          GetHitObject().TryGetComponent(out Ground ground);
          return ground;
     }

     IEnumerator RotationMovement(float yValue)
     {

          Vector3 rotate = doorObje.rotation.eulerAngles;
          rotate.y += yValue;
          bool rotated = false;
          while (!rotated)
          {
               doorObje.DORotate(rotate, duration).OnComplete(() =>
               {
                    rotated = true;
               });
               yield return null;
          }

     }

     public void StatusController()
     {
          Status = !Status;

          if (Status)
          {
               ground().isMoveActive = true;
               Started();
          }
          else
          {
               ground().isMoveActive = false;
               Endend();
          }
     }
}
