using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Player playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerController = Player.Instance;
    }

    private void OnMove(InputValue value)
    {
        playerController.MoveInput = value.Get<Vector2>();
    }

    private void OnLook(InputValue value)
    {
        playerController.LookInput = value.Get<Vector2>();
    }

    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            playerController.Interact();
        }
    }
}
