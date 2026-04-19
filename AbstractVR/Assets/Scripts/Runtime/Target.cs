using UnityEngine;

public class Target : MonoBehaviour
{
    const string HITBOX_NAME = "TargetHitbox";
    public static int Count { get; private set; }
    
    [SerializeField] float score = 10f;
    [SerializeField] float maxHealth = 1f;

    MeshRenderer[] meshRenderers;
    Collider hitbox;

    EffectWrapper vfx;

    void Awake()
    {
        Debug.Log("Target Awake");
        // subscribe to particle system's OnComplete event to destroy the target after the hitVFXPrefab's particle system finishes
        FindRefereces();
    }

    private void FindRefereces()
    {
        // Cache references to components for performance optimization
        meshRenderers = GetComponentsInChildren<MeshRenderer>();

        // Find the hitbox to use
        // First find by name in children, then fallback to GetComponentInChildren if not found
        Transform hitboxTransform = transform.Find(HITBOX_NAME);
        if (hitboxTransform != null) hitbox = hitboxTransform.GetComponent<Collider>();
        else                         hitbox = GetComponentInChildren<Collider>();
        
        
        vfx = GetComponentInChildren<EffectWrapper>();
        if (vfx != null)
        {
            vfx.OnEffectFinished += OnHitVFXFinished;
        }
    }
    void OnHitVFXFinished() 
    { 

    }


    void OnEnable()
    {
        Count++;
        Debug.Log("Target OnEnable, Count: " + Count);
    }
    
    void OnDisable()
    {
        Count--;
        Debug.Log("Target OnDisable, Count: " + Count);
    }

    private void OnDestroy()
    {
        Debug.Log("Target OnDestroy, Count: " + Count);
    }
}
