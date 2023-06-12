public interface Interactable
{
     bool Status { get; set; }

     public void StatusController();
}

public interface IWalkable
{

}

public interface IAnimation
{
     public string OpenButtonTrigger();
     public string CloseButtonTrigger();
}
