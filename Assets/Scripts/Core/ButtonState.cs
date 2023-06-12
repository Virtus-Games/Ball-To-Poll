using UnityEngine;



public class ButtonState : MonoBehaviour, Interactable, IAnimation, IWalkable
{
     [SerializeField] private Animator animator;
     [SerializeField] private DoorState doorState;

     private bool status = false;
     public bool Status { get => status; set => status = value; }

     public string CloseButtonTrigger() => "Close";
     public string OpenButtonTrigger() => "Open";

     public void Endend()
     {
          animator.SetTrigger(CloseButtonTrigger());
                    doorState.StatusController();


     }
     public void Started()
     {
          animator.SetTrigger(OpenButtonTrigger());
          doorState.StatusController();
     }

     public void Uptaded()
     {

     }

     public void StatusController()
     {
          Status = !Status;

          if (Status) Started();
          else Endend();

     }
}
