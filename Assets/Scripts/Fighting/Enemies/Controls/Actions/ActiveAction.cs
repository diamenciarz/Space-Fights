using static EntityInput;

public abstract class ActiveAction : ShipAction
{
    public abstract void applyAction(ActionData actionData);
    public override void callAction(ActionData actionData)
    {
        applyAction(actionData);
    }
}
