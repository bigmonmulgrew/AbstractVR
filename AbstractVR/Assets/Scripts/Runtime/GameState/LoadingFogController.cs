using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingFogController : MonoBehaviour
{
    // TODO, was originally going to make this fog fade, but that wasn't working so moving on for now, will come back when time allows.
    [Header("Fade Settings")]
    [SerializeField] float fogFadeDuration = 0.75f;
    [SerializeField] float minFogBlackoutTime = 0.5f;
    [SerializeField] float firstRunDelay = 5f;
    [SerializeField] GameObject blackoutSphere;
    [Tooltip("Larger values will start the sphere further away, but may cause more noticeable pop-in. Adjust based on your scene scale and needs.")]
    [SerializeField] float maxScale = 100f;


    Vector3 startingScale;
    MeshRenderer sphereRenderer;
    Material sphereMaterial;

    bool BlackoutSphereValid => blackoutSphere != null && sphereRenderer != null && sphereMaterial != null;

    void Awake()
    {
        if (blackoutSphere == null)
        {
            Debug.LogWarning("LoadingFogController: No blackoutSphere assigned.");
            return;
        }
                
        startingScale = blackoutSphere.transform.localScale;
        sphereRenderer = blackoutSphere.GetComponent<MeshRenderer>();
        
        if (sphereRenderer == null)
        {
            Debug.LogWarning("LoadingFogController: blackoutSphere has no MeshRenderer.");
            return;
        }

        sphereMaterial = sphereRenderer.material;   // Assuming only 1 material.

        StartCoroutine(DelayedFadeOut(firstRunDelay)); // Start with the fog hidden.
    }

    public IEnumerator DelayedFadeOut(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        yield return FogOut();
    }

    public IEnumerator FogIn()
    {
        yield return FadeTo(1f);
    }

    public IEnumerator FogOut()
    {
        yield return FadeTo(0f);
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        if (!BlackoutSphereValid)
        {
            Debug.LogWarning("LoadingFogController: Blackout sphere is not properly set up.");
            yield break;
        }

        if (targetAlpha == 1f) blackoutSphere.SetActive(true);

        float startAlpha = sphereMaterial.color.a;
        float startScale = targetAlpha == 1f ? 1f : maxScale;                            // Start scale is max when fading in, and normal when fading out.
        if (startAlpha == 1) yield return new WaitForSecondsRealtime(minFogBlackoutTime * 0.5f); // We wait for half the blackout time as this hapens on fade in and fade out.
        float elapsed = 0f;


        while (elapsed < fogFadeDuration)
        {
            elapsed += Time.deltaTime;
            
            // Time step
            float t = Mathf.Clamp01(elapsed / fogFadeDuration);
            float t2 = 1f - Mathf.Pow(1f - t, 3f);                  // Create a smoother ease-out curve for the alpha transition.

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t2);      
            float scaleT = targetAlpha == 1f ? 1f - t : t;      // Scale up when fading in, down when fading out.

            SetSphereScale(scaleT);
            SetSphereAlpha(alpha);
            yield return null;
        }

        if (targetAlpha == 1) yield return new WaitForSecondsRealtime(minFogBlackoutTime * 0.5f);
        
        
        SetSphereAlpha(targetAlpha);
        SetSphereScale(1 - targetAlpha);

        if (targetAlpha == 0f) blackoutSphere.SetActive(false);
        
    }

    void SetSphereAlpha(float alpha)
    {
        Color color = sphereMaterial.color;
        color.a = alpha;
        sphereMaterial.color = color;
    }

    void SetSphereScale(float scale)
    {
        // Map scale 0 to 1 to 1 to max scale
        float mappedScale = Mathf.Lerp(1f, maxScale, scale);
        blackoutSphere.transform.localScale = startingScale * mappedScale;
       
    }
}