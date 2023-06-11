using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

     public AnimationController enemyAnimation;
     public void Attack(GameObject player)
     {
          enemyAnimation.SetTrigger("attack");
          // UI FAİL
          Destroy(player, 3f);
     }

}
