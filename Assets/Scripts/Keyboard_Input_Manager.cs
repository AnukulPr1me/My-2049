using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBoardInputManager : IInputManager
{
    private int _lastXInput;

    private int _lastYInput;

    public InputResult GetInput()
    {
        InputResult result = new InputResult();

        var xInput = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        var yInput = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));

        if (_lastXInput == 0 && _lastYInput == 0)
        {
            result.xInput = xInput;
            result.yInput = yInput;
        }
        _lastXInput = xInput;
        _lastYInput = yInput;

        return result;
    }
}

public class InputResult
{
    public int xInput = 0;
    public int yInput = 0;

    public bool HasValue => xInput != 0 && yInput != 0;
}