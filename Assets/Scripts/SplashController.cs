using System.Collections;
using UnityEngine;

public class SplashController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem splashParticles;
    
    [Tooltip("Reference to the Light component.")]
    [SerializeField] private Light splashLight;

    [Header("Settings")]
    [SerializeField] private float lifetime = 1.0f;
    [SerializeField] private float initialLightRange = 2.0f;

    private void OnEnable()
    {
        if (splashLight != null)
        {
            splashLight.range = initialLightRange;
            splashLight.enabled = true;
        }
        if (splashParticles != null)
            splashParticles.Play();
        StartCoroutine(HandleSplashLifecycle());
    }

    private IEnumerator HandleSplashLifecycle()
    {
        float elapsedTime = 0f;

        while (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
            float percentageComplete = elapsedTime / lifetime;

            if (splashLight != null)
                splashLight.range = Mathf.Lerp(initialLightRange, 0f, percentageComplete);
            
            yield return null; 
        }
        if (splashLight != null) splashLight.range = 0f;

        DisableObject();
    }

    private void DisableObject()
    {
        gameObject.SetActive(false);
    }
}