
using System.Linq;

public class MultiplaeInputManager : IInputManager
{
    private IInputManager[] _managers;

    public MultiplaeInputManager(params IInputManager[] managers)
    {
        _managers = managers;
    }
    public InputResult GetInput()
    {
        var inputResults = _managers.Select(manager => manager.GetInput());
        InputResult result = inputResults.FirstOrDefault(input => input.HasValue);
        return result ?? new InputResult();

    }
}