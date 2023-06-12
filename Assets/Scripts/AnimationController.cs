using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
     public bool isCharaackter = false;
     private Animator animator;

     private void Start() {
          animator = GetComponent<Animator>();
     }
     public void SetTrigger(string trigger)
     {
          if (!isCharaackter) return;
          animator.SetTrigger(trigger);
     }

     public void Move(bool value)
     {
          if (!isCharaackter) return;
          animator.SetBool("move", value);
     }
     public void Idle(bool value)
     {
          if (!isCharaackter) return;
          animator.SetBool("idle", value);
     }
     public void Die(bool value)
     {
          if (!isCharaackter) return;
          animator.SetBool("die", value);
     }
}
