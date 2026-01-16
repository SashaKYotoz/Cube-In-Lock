using System.Collections;
using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] private Vector3 pointOfTeleport;
    [SerializeField] private GameObject linkedPortal;
    [SerializeField] private Material dissolve;
    private Material playerTMP;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && linkedPortal != null)
        {
            GameObject player = collision.gameObject;
            MeshRenderer playerRenderer = player.GetComponent<MeshRenderer>();

            player.transform.SetPositionAndRotation(pointOfTeleport, collision.gameObject.transform.rotation);

            playerTMP = playerRenderer.material;
            
            playerRenderer.material = dissolve;
            
            if (playerTMP.HasProperty("_Color"))
            {
                playerRenderer.material.color = playerTMP.color;
            }
            else if (playerTMP.HasProperty("_BaseColor"))
            {
                playerRenderer.material.SetColor("_BaseColor", playerTMP.GetColor("_BaseColor"));
            }

            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            playerController.canMove = false;
            StartCoroutine(playerController.UnlockingMovement());
            StartCoroutine(PlayerDissolve(playerRenderer));
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (linkedPortal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(linkedPortal.transform.position, pointOfTeleport);
        }
    }

    private IEnumerator PlayerDissolve(MeshRenderer playerRenderer)
    {
        float elapsedTime = 0f;
        float duration = 1.5f;
        float startValue = 0.1f;
        float endValue = 2f;

        Material currentMat = playerRenderer.material;
        currentMat.SetFloat("_CutoffHeight", startValue);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentValue = Mathf.Lerp(startValue, endValue, elapsedTime / duration);
            currentMat.SetFloat("_CutoffHeight", currentValue);
            yield return null;
        }

        currentMat.SetFloat("_CutoffHeight", endValue);
        playerRenderer.material = playerTMP;
    }
}