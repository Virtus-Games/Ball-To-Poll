using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{

     public bool isMoveActive = true;
     
     public bool IsPassFree(){
          return isMoveActive;
     }

     
}