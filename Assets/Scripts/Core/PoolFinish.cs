using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolFinish : MonoBehaviour
{

    private void OnTriggerEnter(Collider other) {
        if(other.TryGetComponent(out PlayerMovement player)){
            player.ScaleMin();
        }
    }
}
