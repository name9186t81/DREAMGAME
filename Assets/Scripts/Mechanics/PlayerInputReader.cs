using System;
using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public enum InputType : byte
    {
        Attack1 = 0,
        Attack2,
        Select1,
        Select2,
        Select3,
        Select4,
        Jump,
        Slide,
    }

    public event Action<InputType> OnInputPressed;
    public event Action<InputType> OnInputHold;
    public event Action<InputType> OnInputReleased;

    private void Update()
    {
        CheckForInput(KeyCode.Mouse0, InputType.Attack1);
        CheckForInput(KeyCode.Mouse1, InputType.Attack2);
        CheckForInput(KeyCode.Alpha1, InputType.Select1);
        CheckForInput(KeyCode.Alpha2, InputType.Select2);
        CheckForInput(KeyCode.Alpha3, InputType.Select3);
        CheckForInput(KeyCode.Alpha4, InputType.Select4);
    }

    private void CheckForInput(KeyCode code, InputType input)
    {
        if (Input.GetKeyDown(code))
        {
            OnInputPressed?.Invoke(input);
        }
        if (Input.GetKeyUp(code))
        {
            OnInputReleased?.Invoke(input);
        }
        if (Input.GetKey(code))
        {
            OnInputHold?.Invoke(input);
        }
    }
}
