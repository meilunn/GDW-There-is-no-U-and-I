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
    
    [Header ("Camera")]
    [SerializeField] private Transform eyes;
    [SerializeField] private float minPitch = -45f;
    [SerializeField] private float maxPitch = 75f;
    private float cameraPitch = 0f;
    private float rotationSpeed = 20.0f;

    [Header ("Gravity")]
    
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float mass = 5.0f;
    
    
    [Header ("Interaction")]
    
    [SerializeField]
    private float interactionRange = 2.0f;
    [SerializeField]
    private LayerMask traceAgainst;
    Interactable currentInteractable;
    
    [Header ("ItemInHand")]
    public MovableInteractable ItemInHand;

    public Transform PlayerHand;
    
    private Vector3 velocity;

    private CharacterController controller;
    
    private Transform cameraMain;

    private PlayerInput playerInput;

    private InputAction movementAction, rotateAction, interactAction;

    public static Player Instance;

     private void Awake() {
        if (Instance != null)
        {
         Destroy(gameObject);
         return;
        }
        Instance = this;
        playerInput = GetComponent<PlayerInput>();
        controller = gameObject.GetComponent<CharacterController>();
        movementAction =  playerInput.actions["Move"];
        rotateAction = playerInput.actions["Look"];
        interactAction = playerInput.actions["Interact"];
    }

    private void OnEnable() {
        movementAction.Enable();
        rotateAction.Enable();
        interactAction.canceled += Interact;
    }

    private void OnDisable() {
        rotateAction.Disable();
        movementAction.Disable();
        interactAction.canceled -= Interact;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cameraMain = Camera.main.transform;
    }
    
    
    private void Move() {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;
        }
        Vector2 movement = movementAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(movement.x, 0f, movement.y);
        
        move = transform.forward * move.z + transform.right * move.x;
        move.y = 0f;
        controller.Move((move + velocity) * Time.deltaTime * playerSpeed );
        
        
    }
    
    private void Rotate()
    {
        //horizontal rotation
        var rotationInput = rotateAction.ReadValue<Vector2>();
        Vector3 startingRotation = transform.rotation.eulerAngles;
        Quaternion rotation  = Quaternion.Euler(startingRotation.x, startingRotation.y + rotationInput.x, startingRotation.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
        
        //vertical rotation
        cameraPitch -= rotationInput.y * rotationSpeed * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
        Quaternion targetPitch = Quaternion.Euler(cameraPitch, 0f, 0f);
        eyes.localRotation = Quaternion.Lerp(eyes.localRotation, targetPitch, Time.deltaTime * rotationSpeed);
    }


    private void Update()
    {
        Rotate();
        RaycastHit hit;
        if (Physics.Raycast(cameraMain.position, cameraMain.forward, out hit, interactionRange, traceAgainst)) {
            Collider coll = hit.collider;
            Interactable newInteractable = coll.GetComponent<Interactable>();
            if (newInteractable != null) 
            {
               ExchangeInteractable(newInteractable);
            }
            else 
            {
                DisableCurrentInteractable();
            }
        }
        else {
            DisableCurrentInteractable();
        }
    }

    private void DisableCurrentInteractable()
    {
        currentInteractable?.TurnInteractable(false);
        currentInteractable = null;
    }
    private void ExchangeInteractable(Interactable newInteractable)
    {
        if (currentInteractable != null)
        {
            if (currentInteractable != newInteractable) 
            {
                currentInteractable.TurnInteractable(false);
                newInteractable.TurnInteractable(true);
                currentInteractable = newInteractable;
            }
        }
        else
        {
            newInteractable.TurnInteractable(true);
            currentInteractable = newInteractable;
        }
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
    
    private void Interact (InputAction.CallbackContext context)
    {
        if (!currentInteractable) return;
        Interactable[] allInteractables = currentInteractable.gameObject.GetComponents<Interactable>();
        foreach (Interactable i in allInteractables)
        {
            if (i.Interact()) break;
        }
    }
    
}
