using UnityEngine;


public interface Interactable
{
     public void Uptaded();
     public void Started();
     public void Endend();
     public bool isEndendHave();
}

public interface IAnimation
{
     public string OpenButtonTrigger();
     public string CloseButtonTrigger();
}

public class ButtonState : MonoBehaviour, Interactable, IAnimation
{
     [SerializeField] private Animator animator;
     [SerializeField] private DoorState doorState;

     public string CloseButtonTrigger() => "Close";
     public string OpenButtonTrigger() => "Open";

     public void Endend()
     {
          if (doorState.isEndendHave()) animator.SetTrigger(CloseButtonTrigger());
     }
     public void Started()
     {
          animator.SetTrigger(OpenButtonTrigger());
          doorState.Started();
     }
     public void Uptaded()
     {

     }

     public bool isEndendHave() => true;

}
