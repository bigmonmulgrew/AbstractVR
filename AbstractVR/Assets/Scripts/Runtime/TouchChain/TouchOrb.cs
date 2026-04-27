using UnityEngine;

public class TouchOrb : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] int score = 10;
    [SerializeField] float targetTime = 1.0f;
    [SerializeField] float timingTolerancePercent = 0.1f;
    [SerializeField] float waitingScale = 0.5f;
    [SerializeField] float activeScale = 1.0f;

    [Header("Lifetime")]
    [SerializeField]  float maxTime = 3.0f;

    [Header("Detection")]
    [SerializeField] private LayerMask handLayerMask;

    [Header("Visuals")]
    [SerializeField] private Transform orb;
    [SerializeField] private Transform progressOrb;
    [SerializeField] private Vector3 progressStartScale = Vector3.zero;
    [SerializeField] private Vector3 progressFullScale = Vector3.one;

    TouchChain chain;
    int index;

    bool active;
    bool completed;
    float timer;

    public void Initialize(TouchChain owner, int orbIndex)
    {
        chain = owner;
        index = orbIndex;
    }

    void Update()
    {
        if (!active || completed) return;

        timer += Time.deltaTime;

        UpdateProgressVisual();

        if (timer >= maxTime)
        {
            active = false;
            chain.OrbExpired(this, index);
        }
    }

    public void Activate()
    {
        gameObject.SetActive(true);

        active = true;
        completed = false;
        timer = 0f;

        if (orb) orb.transform.localScale = new Vector3(activeScale, activeScale, activeScale);

        UpdateProgressVisual();
    }

    public void SetWaiting()
    {
        active = false;
        completed = false;
        timer = 0f;

        gameObject.SetActive(true);

        if (progressOrb != null) progressOrb.localScale = progressStartScale;
        if (orb) orb.transform.localScale = new Vector3(waitingScale, waitingScale, waitingScale);
    }

    public void Complete()
    {
        active = false;
        completed = true;

        // Hide immediately:
        gameObject.SetActive(false);

        // TODO Consider leaving visible and change material/colour here.
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TouchOrb trigger entered by {other.name}, layer {LayerMask.LayerToName(other.gameObject.layer)}");

        if (!active || completed)
        {
            Debug.Log("Touch ignored because orb is not active.");
            return;
        }

        if (!IsHandOrController(other.gameObject))
        {
            Debug.Log("Touch ignored because object is not on hand layer.");
            return;
        }

        int awardedScore = CalculateScore();
        chain.OrbTouched(this, index, awardedScore);
        GameManager.Instance.Score(awardedScore);
    }

    bool IsHandOrController(GameObject obj)
    {
        return (handLayerMask.value & (1 << obj.layer)) != 0;
    }

    int CalculateScore()
    {
        float minTime = targetTime * (1f - timingTolerancePercent);
        float maxTime = targetTime * (1f + timingTolerancePercent);

        if (timer >= minTime && timer <= maxTime) return score;

        return 0;
    }

    void UpdateProgressVisual()
    {
        if (progressOrb == null) return;

        float progress = Mathf.Clamp01(timer / targetTime);
        progressOrb.localScale = Vector3.Lerp(progressStartScale, progressFullScale, progress);
    }
}