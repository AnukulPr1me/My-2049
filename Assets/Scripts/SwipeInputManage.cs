
using System;
using UnityEngine;

public class SwipeInputManager : IInputManager
{

    private bool isSwiping = false;

    private Vector3 _start_pos;

    private const int MinSwipeDist = 100;
    public InputResult GetInput()
    {
        InputResult result = new InputResult();

        if (!isSwiping)
        {
            if(Input.GetMouseButton(0))
            {
                isSwiping = true;
                _start_pos = Input.mousePosition;
            }
        }
        else
        {
            if (!Input.GetMouseButton(0))
            {
                isSwiping = false;
                Vector3 delta = Input.mousePosition - _start_pos;

                if (delta.magnitude >= MinSwipeDist)
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        result.xInput = Math.Sign(delta.x);
                    }
                    else
                    {
                        result.yInput = Math.Sign(delta.y);
                    }
                }
            }
        }
        return result;
    }
}