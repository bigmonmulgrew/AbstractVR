using UnityEngine;

public class AimingReticle : MonoBehaviour
{
    public static AimingReticle Instance;
    #region Configuration
    [SerializeField] Mesh reticuleMesh;
    [SerializeField] Material HandAimMaterial;
    [SerializeField] Material HeadAimMaterial;
    [SerializeField] Material CombinedAimMaterial;

    [SerializeField] float reticuleDistance = 5f;
    [SerializeField] float moveSpeed = 5f;

    #endregion

    #region References
    GameObject handReticule;
    GameObject headReticule;
    GameObject combinedReticule;

    #endregion

    #region Preallocations
    // These are preallocated as a performance optimisaiton.
    Vector3 handReticulePos;
    Vector3 headReticulePos;
    Vector3 combinedReticulePos;
    #endregion

    #region Accessors
    public GameObject HandReticule => handReticule;
    public GameObject HeadReticule => headReticule;
    public GameObject CombinedReticule => combinedReticule;
    #endregion
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("AimingReticle: Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (reticuleMesh == null)
        {
            Debug.LogWarning("AimingReticle: No reticuleMesh assigned.");
            return;
        }

        CreateReticule(HandAimMaterial, out handReticule, Vector3.left);
        CreateReticule(HeadAimMaterial, out headReticule, Vector3.right);
        CreateReticule(CombinedAimMaterial, out combinedReticule);

    }
    void CreateReticule(Material reticuleMaterial, out GameObject newReticule, Vector3 offset = new()) 
    {
        if (reticuleMaterial == null)
        {
            Debug.LogWarning("AimingReticle: No reticuleMaterial assigned.");
            newReticule = null;
            return;
        }

        // Create new game object for the reticule
        newReticule = new GameObject("Reticule");
        newReticule.transform.SetParent(transform);
        newReticule.transform.localPosition = Vector3.forward * reticuleDistance + offset;
        newReticule.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = newReticule.AddComponent<MeshFilter>();
        meshFilter.mesh = reticuleMesh;

        MeshRenderer meshRenderer = newReticule.AddComponent<MeshRenderer>();
        meshRenderer.material = reticuleMaterial;

    }

    private void Update()
    {
        // Move the combined reticule towards the average position of the hand and head reticules
        handReticulePos = handReticule.transform.localPosition;
        headReticulePos = headReticule.transform.localPosition;

        // Manually calculate X and Y with fixed z incase of errors
        combinedReticulePos.x  = (handReticulePos.x + headReticulePos.x) * 0.5f;
        combinedReticulePos.y  = (handReticulePos.y + headReticulePos.y) * 0.5f;
        combinedReticulePos.z  = reticuleDistance;

        combinedReticule.transform.localPosition = Vector3.Lerp(combinedReticule.transform.localPosition, combinedReticulePos, moveSpeed * Time.deltaTime);

    }
}
