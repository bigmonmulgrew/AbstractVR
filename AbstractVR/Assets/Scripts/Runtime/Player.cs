using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] GameObject weapon;
    [SerializeField] GameObject head;

    XRIDefaultInputActions playerInputs;
    InputAction headLook;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple players detected");
        }


    }
    private void Update()
    {
        if (playerInputs.XRIPlayerInputs.Fire.WasPerformedThisFrame()) Debug.Log("Fire triggered");
        if (playerInputs.XRIPlayerInputs.Fire.WasPerformedThisFrame()) Debug.Log("Fire triggered");
    }
}
