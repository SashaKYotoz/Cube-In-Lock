using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayersManager : MonoBehaviour
{
    public PlayerMovement[] allPlayers;
    public CameraFollow cameraScript;

    [Header("Input")]
    public InputActionReference switchAction;

    [SerializeField] private GameObject fogPlane;

    private int currentIndex = 0;

    void Start()
    {
        if (allPlayers.Length == 0) return;

        for (int i = 0; i < allPlayers.Length; i++)
        {
            allPlayers[i].SetPlayerState(i == 0);
        }

        if (cameraScript != null)
        {
            cameraScript.player = allPlayers[0].transform;
        }
        allPlayers[0].SetPlayerState(true);
        StartCoroutine(PlaceFogBetween());
    }

    private void OnEnable()
    {
        if (switchAction != null)
        {
            switchAction.action.Enable();
            switchAction.action.performed += OnSwitchPressed;
        }
    }

    private void OnDisable()
    {
        if (switchAction != null)
        {
            switchAction.action.Disable();
            switchAction.action.performed -= OnSwitchPressed;
        }
    }

    private void OnSwitchPressed(InputAction.CallbackContext context)
    {
        allPlayers[currentIndex].SetPlayerState(false);
        currentIndex = (currentIndex + 1) % allPlayers.Length;
        allPlayers[currentIndex].SetPlayerState(true);

        if (cameraScript != null)
        {
            cameraScript.player = allPlayers[currentIndex].transform;
        }
    }
    private IEnumerator PlaceFogBetween()
{
    while (true)
    {
        Vector3 centerPosition = Vector3.zero;

        foreach (PlayerMovement player in allPlayers)
        {
            centerPosition += player.transform.position;
        }

        if (allPlayers.Length > 0)
        {
            centerPosition /= allPlayers.Length;
            fogPlane.transform.position = new Vector3(centerPosition.x, -40f, centerPosition.z);
        }

        yield return new WaitForSeconds(1f);
    }
}
}