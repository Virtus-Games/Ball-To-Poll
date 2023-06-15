using DG.Tweening;
using UnityEngine;


public class PlayerMovement : Singleton<PlayerMovement>, IManagerMove
{

     [Header("Jump Settings")]
     [SerializeField] private float duration;
     [SerializeField] private int numJumps;
     [SerializeField] private float jumpPower;
     private Vector3 JumpVector;
     public Vector3 itemGroundPosition { get => JumpVector; set => JumpVector = value; }

     [Header("Move Settings")]
     private float yStartPos = 0;
     private MoveType _moveType;
     internal MoveType GetMoveType() => _moveType;
     internal void SetMoveType(MoveType moveType) => _moveType = moveType;
     public MoveType MoveType { get => _moveType; set => _moveType = value; }

     private void Start()
     {
          DOTween.Init();
          yStartPos = transform.position.y;
     }

     internal void MoveCharacter() => transform.DOJump(Jump(), jumpPower, numJumps, duration);

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
          throw new System.NotImplementedException();
     }
}