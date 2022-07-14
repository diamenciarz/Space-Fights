using static ActionController;
using static MovementScheme;

public interface IActionControllerCaller
{
    public abstract bool IsOn(EntityInputs input);
    public abstract ActionControllerData GetData();
}
