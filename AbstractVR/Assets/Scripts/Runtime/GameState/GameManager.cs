using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("GameManager");
                    instance = obj.AddComponent<GameManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    XRIDefaultInputActions debugInputs;
    InputAction killTargets;

    int score = 0;

    private void Awake()
    {
        CreateInstance();
        debugInputs = new XRIDefaultInputActions();
        killTargets = debugInputs.Debugging.KillAllTargets;

    }
    private void OnEnable()
    {
        debugInputs.Enable();
    }


    private void OnDisable()
    {
        debugInputs.Disable();
    }
    private void Update()
    {
        if (killTargets.WasPressedThisFrame())
        {
            Debug.Log("Kill all targets");
            Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
            foreach (Target target in targets)
            {
                target.Hit();
            }
        }
    }
    private void CreateInstance()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}
