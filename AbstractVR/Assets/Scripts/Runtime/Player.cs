using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    public static Player Instance;

    [SerializeField] GameObject weapon;
    [SerializeField] GameObject head;
    [SerializeField] float shotRange = 20.0f;
    [SerializeField] float weaponCooldown = 1.0f;

    XRIDefaultInputActions playerInputs;
    InputAction headLook;

    LaserBeam headLaser;
    LaserBeam handLaser;
    LaserBeam combinedLaser;

    float weaponActiveTime = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple players detected");
            return;
        }

        playerInputs = new XRIDefaultInputActions();

    }
    private void Start()
    {
        headLaser = AimingReticle.Instance.HeadReticule.GetComponentInChildren<LaserBeam>(true);
        handLaser = AimingReticle.Instance.HandReticule.GetComponentInChildren<LaserBeam>(true);
        combinedLaser = AimingReticle.Instance.CombinedReticule.GetComponentInChildren<LaserBeam>(true);
    }
    private void OnEnable()
    {
        playerInputs.Enable();
    }
    void OnDisable()
    {
        playerInputs.Disable();
    }
    private void Update()
    {
        if (playerInputs.XRIPlayerInputs.Fire.WasPerformedThisFrame()) FireWeapons();
    }

    void FireWeapons()
    {
        if (Time.time < weaponActiveTime) return;

        weaponActiveTime = Time.time + weaponCooldown;
        
        
        FireSingle(headLaser, playerInputs.XRIHead.Position.ReadValue<Vector3>(), AimingReticle.Instance.HeadForward * shotRange + AimingReticle.Instance.HeadPosition);
        FireSingle(handLaser, playerInputs.XRIRight.Position.ReadValue<Vector3>(), AimingReticle.Instance.HandForward * shotRange + AimingReticle.Instance.HandPosition);
        FireSingle(combinedLaser, AimingReticle.Instance.HeadReticule.transform.position, AimingReticle.Instance.HandReticule.transform.position);
    }
    void FireSingle(LaserBeam laser, Vector3 origin, Vector3 end)
    {
        laser.Trigger(end, origin);
        
        // Check for mpacts
        if (Physics.Linecast(origin, end, out RaycastHit hit, Target.HIT_LAYERS + ShootUI.HIT_LAYERS))
        {
            if (hit.collider.transform.parent.TryGetComponent<Target>(out Target target))
            {
                
                target.Hit(hit.point);
            }
            else if (hit.collider.transform.parent.TryGetComponent<ShootUI>(out ShootUI shootUI)) shootUI.Hit();

        }
    }
}
