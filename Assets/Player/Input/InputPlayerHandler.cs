using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPlayerHandler : MonoBehaviour
{

    private Vector2 movmentInput;

    public void OnMoveInput(InputAction.CallbackContext context)
    {

        movmentInput = context.ReadValue<Vector2>();
        Debug.Log(movmentInput);
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {

        if (context.started)
        {
            Debug.Log("jum button is pusehed down now");
        }

        if (context.performed)
        {
            Debug.Log("jump button is being held down");
        }

        if (context.canceled)
        {
            Debug.Log("jump button has been released");
        }
    }
}
