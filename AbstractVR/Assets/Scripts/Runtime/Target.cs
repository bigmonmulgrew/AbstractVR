using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public static readonly LayerMask HIT_LAYERS = 2048; //  Debug.Log(LayerMask.GetMask("TargetHitbox"));

    const string HITBOX_NAME = "TargetHitbox";
    public static int Count { get; private set; }
    
    [SerializeField] int score = 10;
    [SerializeField] int maxHealth = 1;
    [SerializeField] ParticleSystem hitEffectPrefab;

    ParticleSystem hitEffect;
    Coroutine hitEffectCoroutine;

    MeshRenderer[] meshRenderers;
    Collider meshCollider;
    Collider hitbox;
    Rigidbody rb;

    int currentHealth;
    float hitEffectDuration;

    void Awake()
    {
        // subscribe to particle system's OnComplete event to destroy the target after the hitVFXPrefab's particle system finishes
        FindRefereces();
        currentHealth = maxHealth;

        hitEffect = Instantiate(hitEffectPrefab, transform.position, transform.rotation, transform);
        hitEffect.Stop();
        hitEffect.gameObject.SetActive(false);
        hitEffectDuration = hitEffect.main.duration;
    }

    private void FindRefereces()
    {
        // Cache references to components for performance optimization
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        meshCollider = GetComponentInChildren<MeshCollider>();

        rb = GetComponent<Rigidbody>();

        // Find the hitbox to use
        // First find by name in children, then fallback to GetComponentInChildren if not found
        Transform hitboxTransform = transform.Find(HITBOX_NAME);
        if (hitboxTransform != null) hitbox = hitboxTransform.GetComponent<Collider>();
        else                         hitbox = GetComponentInChildren<Collider>();
        
        
    }
    IEnumerator PlayHitEffect()
    {
        hitEffect.gameObject.SetActive(true);
        hitEffect.Play();
        yield return new WaitForSeconds(hitEffectDuration);
        hitEffect.Stop();
        hitEffect.gameObject.SetActive(false);

    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
    }
    public int Hit(int damage = 1)
    {
        currentHealth -= damage;
        
        if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);
        hitEffectCoroutine = StartCoroutine(PlayHitEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
        return score;
    }
    void Die()
    {
        rb.isKinematic = false;
        
        

        hitbox.enabled = false;         // Disable hitbox for shooting
        meshCollider.enabled = true;    // Enable mesh collision for rag doll

    }
    
}
