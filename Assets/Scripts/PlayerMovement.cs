using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Vector3 initPoint;
    [SerializeField] private Color effectsColor;
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

    [Header("Effects Prefabs")]
    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private GameObject lighterPrefab;
    [SerializeField] private float lighterLerpSpeed = 10f;

    private GameObject splashObject;
    private GameObject lighterObject;
    private static WaitForSeconds waitForSeconds = new(2.5f);

    private Rigidbody rb;
    private bool isGrounded;
    private Vector2 inputDirection;

    public bool canMove;
    public bool isControlled = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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

        StartCoroutine(UnlockingMovement());
    }

    private void OnEnable()
    {
        if (isControlled)
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (splashObject != null) Destroy(splashObject);
        if (lighterObject != null) Destroy(lighterObject);
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
                splashObject.transform.position = transform.position + new Vector3(1.5f, -0.75f, 1.5f);
                splashObject.transform.rotation = Quaternion.identity;
            }
        }

        isGrounded = isTouchingGround;
        inputDirection = moveAction.action.ReadValue<Vector2>();

        if (inputDirection.sqrMagnitude > 0.01f || rb.linearVelocity.y > 0)
        {
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
            moveAction.action.Enable();
            jumpAction.action.Enable();
        }
        else
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();

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