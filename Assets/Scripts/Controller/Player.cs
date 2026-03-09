using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{

    [Header ("Movement")]

    [SerializeField]

    private float playerSpeed = 10.0f;


    [Header ("Gravity")]
    
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float mass = 5.0f;

    private Vector3 velocity;

    private CharacterController controller;
    
    private Transform cameraMain;

    private PlayerInput playerInput;

    private InputAction movementAction, rotateAction;


     void Awake() {
        Cursor.visible = false;
        playerInput = GetComponent<PlayerInput>();
        controller = gameObject.GetComponent<CharacterController>();
        movementAction =  playerInput.actions["Move"];
        rotateAction = playerInput.actions["Look"];
    }

    private void OnEnable() {
        movementAction.Enable();
        rotateAction.Enable();
    }



    private void OnDisable() {
        rotateAction.Disable();
        movementAction.Disable();
 
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraMain = Camera.main.transform;
    }


    
    private void Move() {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        Vector2 movement = movementAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        move = cameraMain.forward * move.z + cameraMain.right * move.x;
        move.y = 0f;
        controller.Move((move + velocity) * Time.deltaTime * playerSpeed );
    }

   
    void FixedUpdate()
    {
        UpdateGravity();
        Move();
    }

    void UpdateGravity() {
        var gravity = Physics.gravity * mass * Time.deltaTime;
        velocity.y =  controller.isGrounded ? -1f : gravity.y;
    }
}
