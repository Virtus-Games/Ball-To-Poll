using UnityEngine;

public class DoorState : MonoBehaviour, Interactable, IAnimation
{
     [SerializeField] private Animator animator;
     [SerializeField] private bool openDoor = false;


     private void Start()
     {
     }

     public void Endend()
     {
          if (isEndendHave()) SetTrigger(CloseButtonTrigger());
     }


     public void Started()
     {
          if (!openDoor)
               SetTrigger(OpenButtonTrigger());
            
          openDoor = true;
     }

     public void Uptaded()
     {

     }

     public string CloseButtonTrigger() => "Close";
     public string OpenButtonTrigger() => "Open";
     private void SetTrigger(string str) => animator.SetTrigger(str);
     public bool isEndendHave() => false;

}
