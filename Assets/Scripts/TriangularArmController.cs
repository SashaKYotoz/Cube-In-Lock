using UnityEngine;

public class TriangularArmController : MonoBehaviour
{
    private Vector3 targetScale;
    private Vector3 originalScale;
    private readonly float fadeSpeed = 5f;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * fadeSpeed);

        if (transform.localScale.x < 0.01f && targetScale == Vector3.zero)
        {
            gameObject.SetActive(false);
        }
    }

    public void SetVisibility(bool isVisible)
    {
        if (isVisible)
        {
            gameObject.SetActive(true);
            targetScale = originalScale;
        }
        else
        {
            targetScale = Vector3.zero;
        }
    }
}