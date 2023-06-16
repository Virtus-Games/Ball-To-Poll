using UnityEngine;
using DG.Tweening;

public class FollowPlayer : Singleton<FollowPlayer>
{

     // Transform of the camera to shake. Grabs the gameObject's transform
     // if null.
     private Transform camTransform;

     // How long the object should shake for.
     private float originalshakeDuration = 0f;
     public float shakeDuration = 0f;

     // Amplitude of the shake. A larger value shakes the camera harder.
     public float shakeAmount = 0.7f;
     public float decreaseFactor = 1.0f;

     Vector3 originalPos;
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
               transform.DOMoveZ(offset.z, Speed);
     }




     private void Start()
     {
          if (camTransform == null)
               camTransform = GetComponent(typeof(Transform)) as Transform;

          originalPos = camTransform.localPosition;
        
     }

     void OnEnable()
     {
          originalshakeDuration = shakeDuration;
     }

     void Update()
     {
          Shake();
     }

     internal void ShakeActive()
     {
          shakeDuration = originalshakeDuration;
     }

     private void Shake()
     {
          if (shakeDuration > 0)
          {
               camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

               shakeDuration -= Time.deltaTime * decreaseFactor;
          }
          else
          {
               shakeDuration = 0f;
               camTransform.localPosition = originalPos;
          }
     }
}
