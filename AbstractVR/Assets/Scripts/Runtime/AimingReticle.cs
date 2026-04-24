using UnityEngine;
using UnityEngine.InputSystem;

public class AimingReticle : MonoBehaviour
{
    public static AimingReticle Instance;
    #region Configuration
    [SerializeField] Mesh reticuleMesh;
    [SerializeField] Material HandAimMaterial;
    [SerializeField] Material HeadAimMaterial;
    [SerializeField] Material CombinedAimMaterial;
    [SerializeField] LaserBeam laserPrefab;

    [SerializeField] float reticuleDistance = 5f;
    [SerializeField] float moveSpeed = 5f;

    #endregion

    #region References
    XRIDefaultInputActions playerInputs;
    InputAction headRotation;
    InputAction headPositionInput;
    InputAction handRotation;
    InputAction handPositionInput;
    InputAction fireInput;

    GameObject handReticule;
    GameObject headReticule;
    GameObject combinedReticule;

    #endregion

    #region Preallocations
    // These are preallocated as a performance optimisaiton.
    Vector3 handReticulePos;
    Vector3 headReticulePos;
    Vector3 combinedReticulePos;

    Vector3 handPosition;
    Vector3 headPosition;

    Quaternion handReticuleRot;
    Quaternion headReticuleRot;
    Quaternion combinedReticuleRot;
    #endregion

    #region Accessors
    public GameObject HandReticule => handReticule;
    public GameObject HeadReticule => headReticule;
    public GameObject CombinedReticule => combinedReticule;
    public Vector3 HandForward => handReticuleRot * Vector3.forward;
    public Vector3 HandPosition => handPosition;
    public Vector3 HeadPosition => headPosition;
    public Vector3 HeadForward => headReticuleRot * Vector3.forward;
    public Vector3 CombinedForward => combinedReticuleRot * Vector3.forward;
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


        playerInputs = new XRIDefaultInputActions();
        fireInput = playerInputs.XRIPlayerInputs.Fire;
        headRotation = playerInputs.XRIHead.Rotation;
        headPositionInput = playerInputs.XRIHead.Position;
        handRotation = playerInputs.XRIRight.Rotation;
        handPositionInput = playerInputs.XRIRight.Position;

        CreateReticule(HandAimMaterial, out handReticule, Vector3.left);
        CreateReticule(HeadAimMaterial, out headReticule, Vector3.right);
        CreateReticule(CombinedAimMaterial, out combinedReticule);

        CreateLaser(HandAimMaterial, handReticule);
        CreateLaser(HeadAimMaterial, headReticule);
        CreateLaser(CombinedAimMaterial, combinedReticule);
        

    }
    private void OnEnable()
    {
        playerInputs.Enable();
    }
    void OnDisable()
    {
        playerInputs.Disable();
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

    void CreateLaser(Material mat, GameObject parent)
    {
        LaserBeam newLaser = Instantiate(laserPrefab, parent.transform);
        newLaser.Initialise(mat);
    }
    private void Update()
    {
        UpdateHeadReticule();
        UpdateHandReticule();

        MoveMiddleReticule();
        RotateMiddleReticule();

    }

    void UpdateHeadReticule()
    {
        headPosition = headPositionInput.ReadValue<Vector3>();
        headReticuleRot = headRotation.ReadValue<Quaternion>();

        headReticule.transform.rotation = headReticuleRot;

        headReticulePos = headReticuleRot * Vector3.forward * reticuleDistance + headPosition; 

        headReticule.transform.position = headReticulePos;

    }

    void UpdateHandReticule()
    {
        handPosition = handPositionInput.ReadValue<Vector3>();

        handReticuleRot = handRotation.ReadValue<Quaternion>();

        handReticule.transform.rotation = handReticuleRot;

        handReticulePos = handReticuleRot * Vector3.forward * reticuleDistance + handPosition;

        handReticule.transform.position = handReticulePos;
    }
    private void MoveMiddleReticule()
    {
        

        //// Manually calculate X and Y with fixed z incase of errors
        //combinedReticulePos.x = (handReticulePos.x + headReticulePos.x) * 0.5f;
        //combinedReticulePos.y = (handReticulePos.y + headReticulePos.y) * 0.5f;
        //combinedReticulePos.z = reticuleDistance;

        combinedReticulePos = (handReticulePos + headReticulePos) * 0.5f;

        combinedReticule.transform.position = Vector3.Lerp(combinedReticule.transform.position, combinedReticulePos, moveSpeed * Time.deltaTime);
    }


    void RotateMiddleReticule()
    {
        // No need to get retucule rotations again.
        combinedReticuleRot = Quaternion.Slerp(headReticuleRot, handReticuleRot, 0.5f);

        combinedReticule.transform.rotation = combinedReticuleRot;
        
    }
   
}
