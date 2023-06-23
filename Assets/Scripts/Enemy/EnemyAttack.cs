using UnityEngine;
using System.Collections;

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

          player.GetComponent<PlayerMovement>().stop = true;

          enemyAnimation.SetTrigger(enemyAnimation.ATTACK);
          
          StartCoroutine(DieEffect(player));



     }

     IEnumerator DieEffect(GameObject player)
     {
          yield return new WaitForSeconds(dieTime);
          Destroy(player);
          yield return new WaitForSeconds(1);
          GameManagerProjects.Instance.UpdateGameState(GAMESTATE.DEFEAT);
     }

}
