using UnityEngine;

public class Target : MonoBehaviour
{
    const string HITBOX_NAME = "TargetHitbox";
    public static int Count { get; private set; }
    
    [SerializeField] int score = 10;
    [SerializeField] int maxHealth = 1;

    MeshRenderer[] meshRenderers;
    Collider hitbox;
    Rigidbody rb;

    EffectWrapper vfx;

    int currentHealth;

    void Awake()
    {
        Debug.Log("Target Awake");
        // subscribe to particle system's OnComplete event to destroy the target after the hitVFXPrefab's particle system finishes
        FindRefereces();
        currentHealth = maxHealth;
    }

    private void FindRefereces()
    {
        // Cache references to components for performance optimization
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        rb = GetComponent<Rigidbody>();

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
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }
    public int Hit(int damage = 1)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        return score;
    }
    void Die()
    {
        rb.isKinematic = false;
        if (vfx) vfx.Play();

        hitbox.enabled = false;


    }
    
}
