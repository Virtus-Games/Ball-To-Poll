using DG.Tweening;
using UnityEngine;


public class PlayerMovement : Singleton<PlayerMovement>, IManagerMove
{

     [Header("Jump Settings")]
     [SerializeField] private float duration;
     [SerializeField] private int numJumps;
     [SerializeField] private float jumpPower;
     private Vector3 JumpVector;

     [Header("Move Settings")]
     private float yStartPos = 0;
     public Vector3 itemGroundPosition { get => JumpVector; set => JumpVector = value; }
     [SerializeField] private bool stop = false;
     public float ScaleDuration;

     [Header("Move Enum")]
     private MoveType _moveType;
     public MoveType MoveType { get => _moveType; set => _moveType = value; }
     internal void SetMoveType(MoveType moveType) => _moveType = moveType;





     private void Start()
     {
          DOTween.Init();
          yStartPos = transform.position.y;
          FollowPlayer.Instance.SetPlayer();
     }

     internal void MoveCharacter()
     {
          if (!GameManagerProjects.Instance.isPlay) return;
          if (stop) return;
          transform.DOJump(Jump(), jumpPower, numJumps, duration);
          FollowPlayer.Instance.ShakeActive();
     }
     private Vector3 Jump() => new Vector3(JumpVector.x, yStartPos, JumpVector.z);

     public void Run() => MoveCharacter();

     public void Stop()
     {

     }


     public void StopAndRotate()
     {

     }

     public void ChangeMoveType()
     {

     }

     public void ScaleMin()
     {
          stop = true;
          transform.DOScale(Vector3.zero, ScaleDuration).OnComplete(() =>
          {
               GameManagerProjects.Instance.UpdateGameState(GAMESTATE.VICTORY);
          });
     }
}