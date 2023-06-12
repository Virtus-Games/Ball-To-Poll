using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

     private AnimationController enemyAnimation;

     private void Start()
     {
          enemyAnimation = GetComponent<AnimationController>();
     }
     public void Attack(GameObject player)
     {
          enemyAnimation.SetTrigger("attack");
          // UI FAİL
          Destroy(player, 3f);
     }

}
