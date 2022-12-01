using static EntityInput;

public abstract class AbstractActiveAction : AbstractShipAction
{
    public abstract void ApplyAction(ActionData actionData);
    public override void CallAction(ActionData actionData)
    {
        ApplyAction(actionData);
    }
}
