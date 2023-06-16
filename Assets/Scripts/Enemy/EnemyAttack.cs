using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

     private AnimationController enemyAnimation;
     public float dieTime = 1.2f;

     private void Start()
     {
          enemyAnimation = GetComponent<AnimationController>();
     }
     public void Attack(GameObject player)
     {
          enemyAnimation.SetTrigger(enemyAnimation.ATTACK);
          // UI FAİL
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.DEFEAT);
          Destroy(player, dieTime);
     }

}
