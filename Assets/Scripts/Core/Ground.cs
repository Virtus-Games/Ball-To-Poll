using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour, IGround
{

     public bool isMoveActive = true;

     public Vector3 _itemGroundPosition;
     public Vector3 itemGroundPosition { get => _itemGroundPosition; set => _itemGroundPosition = value;}

     public bool IsPassFree(){
          return isMoveActive;
     }

     
}
