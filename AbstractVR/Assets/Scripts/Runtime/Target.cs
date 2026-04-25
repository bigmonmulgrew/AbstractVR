using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    public static readonly LayerMask HIT_LAYERS = 2048; //  Debug.Log(LayerMask.GetMask("TargetHitbox"));
    

    const string HITBOX_NAME = "TargetHitbox";
    public static int Count { get; private set; }
    
    [SerializeField] int killScore = 5;
    [SerializeField] int hitscore = 1;
    [SerializeField] int maxHealth = 1;
    [SerializeField] ParticleSystem hitEffectPrefab;
    [SerializeField] ParticleSystem deathEffectPrefab;

    [Header("Motion settings")]
    [SerializeField] Vector3 moveVelocity;
    [SerializeField] Vector3 rotateVelocity;
    [SerializeField] Vector3 scaleVelocity;
    [SerializeField] float dissapearTime;

    ParticleSystem hitEffect;
    ParticleSystem deathEffect;
    Coroutine hitEffectCoroutine;

    MeshRenderer[] meshRenderers;
    Collider meshCollider;
    Collider hitbox;
    Rigidbody rb;

    int currentHealth;
    float hitEffectDuration;
    float deathEffectDuration;
    bool isDead = false;
    float removalTime = 0;

    void Awake()
    {
        // subscribe to particle system's OnComplete event to destroy the target after the hitVFXPrefab's particle system finishes
        FindRefereces();
        currentHealth = maxHealth;

        hitEffect = Instantiate(hitEffectPrefab, transform.position, transform.rotation, transform);
        hitEffect.Stop();
        hitEffect.Clear();
        hitEffect.gameObject.SetActive(false);
        hitEffectDuration = hitEffect.main.duration;

        deathEffect = Instantiate(deathEffectPrefab, transform.position, transform.rotation, transform);
        deathEffect.Stop();
        deathEffect.Clear();
        deathEffect.gameObject.SetActive(false);
        deathEffectDuration = deathEffect.main.duration;

        Count++;

        removalTime = Time.time + dissapearTime;
    }

    private void Update()
    {
        MoveTarget();
        RotateTarget();
        ScaleTarget();
        Dissapear();
    }

    void Dissapear()
    {
        if (isDead) return;
        if (dissapearTime == 0) return;
        if (Time.time > removalTime)
        {
            Count--;
            Destroy(gameObject);
        }
    }
    void MoveTarget()
    {
        if (moveVelocity.magnitude == 0) return;
        transform.position = transform.position + (moveVelocity * Time.deltaTime);
    }
    void RotateTarget()
    {
        if (rotateVelocity.magnitude == 0) return;
        transform.Rotate(rotateVelocity * Time.deltaTime);
    }
    void ScaleTarget()
    {
        if (scaleVelocity.magnitude == 0) return;
        transform.localScale = transform.localScale + (scaleVelocity * Time.deltaTime);
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

    IEnumerator PlayDeathEffect()
    {
        deathEffect.gameObject.SetActive(true);
        deathEffect.Play();
        yield return new WaitForSeconds(deathEffectDuration);
        deathEffect.Stop();
        deathEffect.gameObject.SetActive(false);
        Destroy(gameObject);
    }
    
    public void Hit()
    {
        Hit(1);
    }
    public void Hit(Vector3 hitPosition)
    {
        Hit(1, hitPosition);
    }
    public void Hit(int damage, Vector3 hitPosition)
    {
        hitEffect.transform.position = hitPosition;
        Hit(damage);
    }
    public void Hit(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        if (hitEffectCoroutine != null) StopCoroutine(hitEffectCoroutine);
        hitEffectCoroutine = StartCoroutine(PlayHitEffect());

        if (currentHealth <= 0)
        {
            GameManager.Instance.Score(killScore, this.gameObject);
            Die();
        }
        else GameManager.Instance.Score(hitscore, this.gameObject);

    }
   
    void Die()
    {
        isDead = true;
        rb.isKinematic = false;
        Count--;

        StartCoroutine(PlayDeathEffect());

        hitbox.enabled = false;         // Disable hitbox for shooting
        meshCollider.enabled = true;    // Enable mesh collision for rag doll

    }

}
