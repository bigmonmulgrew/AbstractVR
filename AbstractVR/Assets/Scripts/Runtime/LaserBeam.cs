using System.Collections;
using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [SerializeField] float lifeTime = 0.5f;
    [SerializeField] float beamThickness = 0.2f;

    MeshRenderer meshRenderer;

    Coroutine liveTimeCoroutine;
    Vector3 target;
    Vector3 origin;
    Material material;
    
    public void Initialise(Material material)
    {
        this.material = material;

        meshRenderer.material = material;
    }

    public void Trigger(Vector3 target, Vector3 origin)
    {
        this.target = target;
        this.origin = origin;
        gameObject.SetActive(true);
    }
    void Awake()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        
        gameObject.SetActive(false);
    }

    
    private void OnEnable()
    {
        if (liveTimeCoroutine != null) StopCoroutine(liveTimeCoroutine);

        SetPoints();

        if (lifeTime > 0 ) liveTimeCoroutine =  StartCoroutine(DelayedDisable());
    }
    
    void SetPoints()
    {

        Vector3 direction = target - origin;

        meshRenderer.gameObject.transform.position = origin + direction * 0.5f;
        meshRenderer.gameObject.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        meshRenderer.gameObject.transform.localScale = new Vector3(beamThickness, direction.magnitude * 0.5f, beamThickness);
    }

    private void Update()
    {

        if (!PauseManager.Instance.IsGamePaused) return;
        SetPoints();
    }
    public void DisableLaser()
    {
        gameObject.SetActive(false);

        // Cleanup
        meshRenderer.gameObject.transform.localScale = Vector3.zero;
    }
    IEnumerator DelayedDisable()
    {
        yield return new WaitForSecondsRealtime(lifeTime);
        DisableLaser();
    }
}
