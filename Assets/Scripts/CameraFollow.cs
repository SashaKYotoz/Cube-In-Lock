using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Input")]
    public InputActionReference zoomInput;
    public InputActionReference FPcameraInput;

    [Header("Settings")]
    [Range(0.01f, 0.5f)]
    public float smoothTime = 0.12f;

    [Header("Zoom")]
    public float zoomSpeed = 0.02f;
    public float minDistance = 10f;
    public float maxDistance = 50f;

    [Header("Close View Settings")]
    public Vector3 closeOffset = new(5, 5, 5);

    private Vector3 offset;
    private Vector3 currentVelocity;

    private bool isCloseView;
    private Vector3 savedOffset;
    private float savedMinDistance;
    private float savedMaxDistance;
    private Quaternion savedRotation;

    private void OnEnable()
    {
        if (zoomInput != null)
            zoomInput.action.Enable();
        if (FPcameraInput != null)
        {
            FPcameraInput.action.Enable();
            FPcameraInput.action.performed += OnCameraSet;
        }
    }

    private void OnDisable()
    {
        if (zoomInput != null)
            zoomInput.action.Disable();
        if (FPcameraInput != null)
        {
            FPcameraInput.action.Disable();
            FPcameraInput.action.performed -= OnCameraSet;
        }
    }

    void Start()
    {
        if (player == null)
            return;

        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        float scrollValue = 0f;

        if (zoomInput != null)
        {
            scrollValue = zoomInput.action.ReadValue<Vector2>().y;
        }

        if (scrollValue != 0f)
        {
            float currentDistance = offset.magnitude;
            currentDistance -= scrollValue * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            offset = offset.normalized * currentDistance;
        }

        Vector3 targetPosition = player.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    private void OnCameraSet(InputAction.CallbackContext context)
    {
        isCloseView = !isCloseView;

        if (isCloseView)
        {
            savedOffset = offset;
            savedMinDistance = minDistance;
            savedMaxDistance = maxDistance;
            savedRotation = transform.rotation;

            minDistance = 7.5f;
            maxDistance = 15f;
            
            transform.rotation = Quaternion.Euler(30, 225, 0);
            offset = closeOffset;
        }
        else
        {
            minDistance = savedMinDistance;
            maxDistance = savedMaxDistance;
            
            transform.rotation = savedRotation;
            offset = savedOffset;
        }
    }
}