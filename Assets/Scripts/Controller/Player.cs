using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour {
	[Header("Movement")]

	[SerializeField] private float playerSpeed = 10.0f;
	[SerializeField] private float acceleration = 15.0f;

	[Header("Camera")]
	[SerializeField] private CinemachineCamera playerCam;
	[SerializeField] private Transform eyes;
	[SerializeField] private float minPitch = -45f;
	[SerializeField] private float maxPitch = 75f;
	private float cameraPitch;
	public float CameraPitch
	{
		get => cameraPitch;
		set => cameraPitch = Mathf.Clamp(value, minPitch, maxPitch);
	}
	[SerializeField] private float rotationSpeed = 1f;

	[Header("Gravity")]

	[SerializeField] private float gravityScale = 3f;


	[Header("Interaction")]

	[SerializeField]
	private float interactionRange = 2.0f;
	[SerializeField]
	private LayerMask traceAgainst;
	Interactable currentInteractable;

	[Header("ItemInHand")]
	public MovableInteractable ItemInHand;

	public Transform PlayerHand;

	private Vector3 velocity;
	private float verticalVelocity;

	private CharacterController controller;


	private PlayerInput playerInput;

	private InputAction movementAction, rotateAction, interactAction;

	public static Player Instance;

	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
		playerInput = GetComponent<PlayerInput>();
		controller = GetComponent<CharacterController>();
		movementAction = playerInput.actions["Move"];
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

	private void Start() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}


	private void Move() {
		if (controller.isGrounded && velocity.y < 0) {
			velocity.y = 0f;
		}

		Vector2 moveInput = movementAction.ReadValue<Vector2>();
		Vector3 motion = transform.forward * moveInput.y + transform.right * moveInput.x;
        motion.y = 0f;
        motion.Normalize();	

		if (motion.sqrMagnitude >= 0.01f)
            velocity = Vector3.MoveTowards(
                velocity,
                motion * playerSpeed,
                acceleration * Time.deltaTime
            );
        else
            velocity = Vector3.MoveTowards(
                velocity,
                Vector3.zero,
                acceleration * Time.deltaTime
            );

		if (controller.isGrounded && verticalVelocity <= 0.01f)
            // important if climbing stairs to keep player stuck to ground
            verticalVelocity = -3f;
        else
            // otherwise player will not fall down fast enough
            verticalVelocity += Physics.gravity.y * gravityScale * Time.deltaTime;

		Vector3 actualVelocity = new(velocity.x, 0f, velocity.z);

		controller.Move(actualVelocity * Time.deltaTime);
	}

	private void Rotate() {
		//horizontal rotation
		var rotationInput = rotateAction.ReadValue<Vector2>() * rotationSpeed;  //rotation speed is sens
		
		// look up and down
        CameraPitch -= rotationInput.y;
        playerCam.transform.localRotation = Quaternion.Euler(CameraPitch, 0f, 0f);

        // look left and right
        transform.Rotate(Vector3.up * rotationInput.x);
	}


	private void Update() {
		Move();
		Rotate();

		RaycastHit hit;
		if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, interactionRange, traceAgainst)) {
			Collider coll = hit.collider;
			Interactable newInteractable = coll.GetComponent<Interactable>();
			if(newInteractable == null && currentInteractable) {
				DisableCurrentInteractable();
			} else if(newInteractable != currentInteractable) {
				ExchangeInteractable(newInteractable);
			}
		} else {
			DisableCurrentInteractable();
		}
	}

	private void DisableCurrentInteractable() {
		currentInteractable?.TurnInteractable(false);
		currentInteractable = null;
	}
	private void ExchangeInteractable(Interactable newInteractable) {
		Debug.Log(newInteractable.gameObject.name);
		if (currentInteractable != null) {
			currentInteractable.TurnInteractable(false);
			newInteractable.TurnInteractable(true);
			currentInteractable = newInteractable;
		} else {
			newInteractable.TurnInteractable(true);
			currentInteractable = newInteractable;
		}
	}

	private void Interact(InputAction.CallbackContext context) {
		if (!currentInteractable) return;
		Interactable[] allInteractables = currentInteractable.gameObject.GetComponents<Interactable>();
		foreach (Interactable i in allInteractables) {
			if (i.Interact()) break;
		}
	}

}
