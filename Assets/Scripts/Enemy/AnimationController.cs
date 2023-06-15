using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
     internal  string MOVE = "move";
     internal  string DIE = "die";
     internal string ATTACK = "attack";
     internal string DAMAGE = "damage";
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

     public void Move()
     {
          if (!isCharaackter) return;
          animator.SetBool(MOVE, true);
     }
     public void Idle()
     {
          if (!isCharaackter) return;
          animator.SetBool(MOVE, false);
     }
     public void Die(bool value)
     {
          if (!isCharaackter) return;
          animator.SetBool(DIE, value);
     }
}
