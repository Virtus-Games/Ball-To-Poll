using UnityEngine;

public class AnimationController : MonoBehaviour
{
     public Animator animator;
     public void SetTrigger(string trigger) => animator.SetTrigger(trigger);
     public void Move(bool value) => animator.SetBool("move", value);
     public void Idle(bool value) => animator.SetBool("idle", value);
     public void Die(bool value) => animator.SetBool("die", value);

}
