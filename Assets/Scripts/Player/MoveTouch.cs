using UnityEngine;

public class MoveTouch : Singleton<MoveTouch>
{
     [HideInInspector]
     public Vector2 swipeDelta, startTouch;
     private const float deadZone = 100;
     private bool tap;

     private void Update()
     {
          TouchController();

          //  Deadzone u geçtik mi
          MoveToPlayer();
     }

     private void MoveToPlayer()
     {
          if (swipeDelta.magnitude > deadZone)
          {
               // evet geçtik
               float x = swipeDelta.x;
               float y = swipeDelta.y;

               if (Mathf.Abs(x) > Mathf.Abs(y))
               {    // sol
                    if (x < 0)
                    {
                         PlayerMovement.Instance.SetMoveType(MoveType.LEFT);
                         PlayerMovement.Instance.MoveCharacter(Vector3.left);
                         
                    }
                    // sağ
                    else
                    {
                         PlayerMovement.Instance.SetMoveType(MoveType.RIGHT);
                         PlayerMovement.Instance.MoveCharacter(Vector3.right);

                    }
               }
               else
               {  // aşağı
                    if (y < 0)
                    {
                         PlayerMovement.Instance.SetMoveType(MoveType.BACK);
                         PlayerMovement.Instance.MoveCharacter(Vector3.back);

                    }   // yukarı
                    else
                    {
                         PlayerMovement.Instance.SetMoveType(MoveType.FORWARD);
                         PlayerMovement.Instance.MoveCharacter(Vector3.forward);
                    }
               }

               startTouch = swipeDelta = Vector2.zero;
          }
     }

     private void TouchController()
     {
          #region Bilgisayar Kontrolleri
          if (Input.GetMouseButtonDown(0))
          {
               tap = true;
               startTouch = Input.mousePosition;
          }
          else if (Input.GetMouseButtonUp(0))
          {
               startTouch = swipeDelta = Vector2.zero;
          }

          #endregion

          #region Mobil Kontrolleri
          if (Input.touches.Length != 0)
          {
               if (Input.touches[0].phase == TouchPhase.Began)
               {
                    tap = true;
                    startTouch = Input.mousePosition;
               }
               else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
               {
                    startTouch = swipeDelta = Vector2.zero;
               }

          }

          #endregion
          // Mesafeyi hesaplıyoruz

          swipeDelta = Vector2.zero;
          if (startTouch != Vector2.zero)
          {
               // Mobil için
               if (Input.touches.Length != 0)
                    swipeDelta = Input.touches[0].position - startTouch;
               // Bilgisayar için
               else if (Input.GetMouseButton(0))
                    swipeDelta = (Vector2)Input.mousePosition - startTouch;
          }
     }
}