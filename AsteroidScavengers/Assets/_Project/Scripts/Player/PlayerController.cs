using UnityEngine;
using Unity.Cinemachine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Mouse Settings")]
    [SerializeField] private float mouseSensitivity = 2f;

    public bool isInSellZone = false;

    private float currentSpeed;

    private CharacterController characterController;

    private Camera playerCamera;
    private float xRotation = 0f;

    private PlayerControls inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;

    private Vector3 velocity;
    private bool isGrounded;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;

        inputActions = new PlayerControls();

        currentSpeed = walkSpeed;
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        ReadInput();

        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isGrounded && inputActions.Player.Jump.triggered)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;

        HandleMovement();
        HandleLook();

        characterController.Move(velocity * Time.deltaTime);
    }

    void ReadInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();


        lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        bool isSprinting = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
    }

    void HandleMovement()
    {
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            Vector3 targetRotation = Vector3.RotateTowards(
                transform.forward,
                moveDirection,
                rotationSpeed * Mathf.Deg2Rad * Time.deltaTime,
                0f
            );

            transform.rotation = Quaternion.LookRotation(targetRotation);

            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
    }

    void HandleLook()
    {
        xRotation -= lookInput.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        walkSpeed = 5f * multiplier;
        sprintSpeed = 10f * multiplier;
    }

    public Vector3 GetPosition() => transform.position;

    [Header("Drone")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private Transform droneSpawnPoint;



    void Start()
    {
        if (dronePrefab != null)
        {
            Vector3 spawnPos = droneSpawnPoint != null ?
                droneSpawnPoint.position :
                transform.position + Vector3.up * 3f + Vector3.forward * 2f;

            Instantiate(dronePrefab, spawnPos, Quaternion.identity);
        }
    }
}