using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Vector3 initPoint;
    [SerializeField] private Color effectsColor;
    [SerializeField] private GameObject triangleArmPref;
    [SerializeField] private GameObject emojiHolderPref;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float rotationMultiplier = 40f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 1.76f;

    [Header("Inputs")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference leftClickAction;
    [SerializeField] private InputActionReference rightClickAction;
    [SerializeField] private InputActionReference mousePositionAction;

    [Header("Effects Prefabs")]
    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private GameObject lighterPrefab;
    [SerializeField] private float lighterLerpSpeed = 10f;

    [Header("Arm Settings")]
    [SerializeField] private Vector3 leftArmOffset = new(-2f, 0.5f, 0f);
    [SerializeField] private Vector3 rightArmOffset = new(2f, 0.5f, 0f);
    [SerializeField] private float armSmoothTime = 10f;
    [SerializeField] private float maxArmReach = 4f;

    private GameObject splashObject;
    private GameObject lighterObject;
    private GameObject emojiObject;
    private TriangularArmController leftArmController;
    private TriangularArmController rightArmController;
    
    private Rigidbody rb;
    private Camera mainCamera;
    private bool isGrounded;
    private Vector2 inputDirection;
    private Vector3 lastMoveDirection = Vector3.forward;
    private static readonly WaitForSeconds waitForSeconds = new(2.5f);

    public bool canMove;
    public bool isControlled = false;
    public bool emojiHaveToShow = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (splashPrefab != null)
        {
            splashObject = Instantiate(splashPrefab, transform.position, Quaternion.identity, null);
            splashObject.GetComponentInChildren<Light>().color = effectsColor;
            splashObject.SetActive(false);
        }

        if (lighterPrefab != null)
        {
            lighterObject = Instantiate(lighterPrefab, transform.position, Quaternion.identity, null);
            lighterObject.GetComponent<Light>().color = effectsColor;
        }

        if (triangleArmPref != null)
        {
            GameObject leftArmObj = Instantiate(triangleArmPref, transform.position, Quaternion.identity, null);
            leftArmController = leftArmObj.GetComponent<TriangularArmController>();
            
            GameObject rightArmObj = Instantiate(triangleArmPref, transform.position, Quaternion.identity, null);
            rightArmController = rightArmObj.GetComponent<TriangularArmController>();
        }
        if(emojiHolderPref != null){
            emojiObject = Instantiate(emojiHolderPref, transform.position, Quaternion.identity, null);
            emojiObject.SetActive(false);
        }

        StartCoroutine(UnlockingMovement());
    }

    private void OnEnable()
    {
        if (isControlled)
        {
            EnableInputs();
        }
    }

    private void OnDisable()
    {
        DisableInputs();
    }

    private void EnableInputs()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        leftClickAction.action.Enable();
        rightClickAction.action.Enable();
        mousePositionAction.action.Enable();
    }

    private void DisableInputs()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        leftClickAction.action.Disable();
        rightClickAction.action.Disable();
        mousePositionAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (splashObject != null) Destroy(splashObject);
        if (lighterObject != null) Destroy(lighterObject);
        if (leftArmController != null) Destroy(leftArmController.gameObject);
        if (rightArmController != null) Destroy(rightArmController.gameObject);
    }

    private void Update()
    {
        if (!isControlled) return;

        if (transform.position.y < -10)
            transform.SetPositionAndRotation(initPoint, Quaternion.identity);

        bool isTouchingGround = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded && !splashObject.activeInHierarchy)
        {
            if (splashObject != null)
            {
                splashObject.SetActive(true);
                splashObject.transform.SetPositionAndRotation(transform.position + new Vector3(1.5f, -0.75f, 1.5f), Quaternion.identity);
            }
        }

        isGrounded = isTouchingGround;
        inputDirection = moveAction.action.ReadValue<Vector2>();

        bool isMoving = inputDirection.sqrMagnitude > 0.01f || Math.Abs(rb.linearVelocity.y) > 0.1f || new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude > 0.1f;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
            
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(isGrounded ? 0 : 270, targetAngle, 0);

            Vector3 moveDir = new Vector3(inputDirection.x, 0, inputDirection.y).normalized;
            Vector3 targetPosition = transform.position + (moveDir * 1.5f) + new Vector3(0, -0.75f, 0);

            if (lighterObject != null)
            {
                lighterObject.transform.SetPositionAndRotation(Vector3.Lerp(lighterObject.transform.position, targetPosition, Time.deltaTime * lighterLerpSpeed),
                Quaternion.Slerp(lighterObject.transform.rotation, targetRotation, Time.deltaTime * lighterLerpSpeed));
            }
        }
        else if (lighterObject != null)
        {
            Vector3 idlePos = transform.position + new Vector3(0, -0.75f, 0);
            lighterObject.transform.position = Vector3.Lerp(lighterObject.transform.position, idlePos, Time.deltaTime * lighterLerpSpeed);
        }

        UpdateArms(isMoving);

        if (jumpAction.action.WasPressedThisFrame() && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (horizontalVelocity.sqrMagnitude > 0.01f)
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, horizontalVelocity.normalized);
            transform.Rotate(rotationAxis, horizontalVelocity.magnitude * rotationMultiplier * Time.deltaTime, Space.World);
        }
    }

    private void UpdateArms(bool isMoving)
    {
        if (leftArmController == null || rightArmController == null) return;

        bool isLeftPressed = leftClickAction.action.IsPressed();
        bool isRightPressed = rightClickAction.action.IsPressed();
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Quaternion faceDirection = Quaternion.LookRotation(lastMoveDirection);

        if (isLeftPressed)
        {
            leftArmController.SetVisibility(true);
            MoveArmToTarget(leftArmController.transform, mouseWorldPos);
        }
        else
        {
            if (isMoving)
            {
                leftArmController.SetVisibility(false);
            }
            else
            {
                leftArmController.SetVisibility(true);
                Vector3 leftTargetPos = transform.position + faceDirection * leftArmOffset;
                LerpArmToIdle(leftArmController.transform, leftTargetPos, faceDirection);
            }
        }

        if (isRightPressed)
        {
            rightArmController.SetVisibility(true);
            MoveArmToTarget(rightArmController.transform, mouseWorldPos);
        }
        else
        {
            if (isMoving)
            {
                rightArmController.SetVisibility(false);
            }
            else
            {
                rightArmController.SetVisibility(true);
                Vector3 rightTargetPos = transform.position + faceDirection * rightArmOffset;
                LerpArmToIdle(rightArmController.transform, rightTargetPos, faceDirection);
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mouseScreenPos = mousePositionAction.action.ReadValue<Vector2>();
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPos);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        
        if (groundPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
        }
        return transform.position;
    }

    private void MoveArmToTarget(Transform armTransform, Vector3 targetWorldPos)
    {
        Vector3 directionToTarget = targetWorldPos - transform.position;
        float dist = Mathf.Min(directionToTarget.magnitude, maxArmReach);
        Vector3 clampedTarget = transform.position + directionToTarget.normalized * dist;
        
        clampedTarget.y = transform.position.y + 0.5f;

        armTransform.position = Vector3.Lerp(armTransform.position, clampedTarget, Time.deltaTime * armSmoothTime);
        armTransform.LookAt(targetWorldPos);
    }

    private void LerpArmToIdle(Transform armTransform, Vector3 targetPos, Quaternion faceDir)
    {
        armTransform.position = Vector3.Lerp(armTransform.position, targetPos, Time.deltaTime * armSmoothTime);
        armTransform.rotation = Quaternion.Slerp(armTransform.rotation, faceDir, Time.deltaTime * armSmoothTime);
    }

    private void FixedUpdate()
    {
        if (canMove && isControlled)
            rb.linearVelocity = new Vector3(inputDirection.x * moveSpeed, rb.linearVelocity.y, inputDirection.y * moveSpeed);
    }

    public void SetPlayerState(bool state)
    {
        isControlled = state;

        if (state)
        {
            EnableInputs();
        }
        else
        {
            DisableInputs();

            if (splashObject != null) splashObject.SetActive(false);

            inputDirection = Vector2.zero;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private IEnumerator UnlockingMovement()
    {
        yield return waitForSeconds;
        canMove = true;
    }
}