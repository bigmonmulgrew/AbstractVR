using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private enum GameMode
    {
        Arcade,
        Competitive
    }

    [System.Serializable]
    struct Limits
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 Offset;
        [HideInInspector] public Bounds Bounds;
    }

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

    [SerializeField] GameMode defaultGameMode;
    [SerializeField] Limits limits;
    [SerializeField] Target[] targetPrefabs;

    [Header("Enemy quantities")]
    [SerializeField] int totalTargets;
    [SerializeField] int maxEnemies;
    [SerializeField] float initialWaitTime = 10.0f;
    [SerializeField] float spawnIntervalMin;
    [SerializeField] float spawnIntervalMax;


    XRIDefaultInputActions debugInputs;
    InputAction killTargets;
    Transform enemyHolder;


    int score = 0;
    GameMode gameMode = GameMode.Arcade;
    float nextSpawnTime = 0;
    bool isSpawning = false;
    int totalSpawned = 0;
    
    List<int> frameScores = new();
    HashSet<GameObject> hitTargetsInFrame = new();

    int ScoreMultiplier => Mathf.Max(frameScores.Count, 1) * Mathf.Max(hitTargetsInFrame.Count * hitTargetsInFrame.Count, 1);

    private void Awake()
    {
        CreateInstance();
        debugInputs = new XRIDefaultInputActions();
        killTargets = debugInputs.Debugging.KillAllTargets;

        SanityChecks();
        CreateEnemyHolder();
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
    void SanityChecks()
    {
        if (targetPrefabs.Length == 0) Debug.LogError("No target prefabs assigned in game manager");
    }
    void CreateEnemyHolder()
    {
        enemyHolder = new GameObject("EnemyHolder").transform; 
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
        DebugInputs();
        SpawnEnemies();
    }

    private void LateUpdate()
    {
        CalculateScores();
    }
    void CalculateScores()
    {
        if (frameScores.Count == 0) return;

        if (gameMode == GameMode.Competitive)
        {
            Debug.Log($"Game Manager: Frame scores count {frameScores.Count}, with highest score {frameScores.Max()}");
            score += frameScores.Max();
        }
        else
        {
            foreach (int scr in frameScores)
            {
                score += scr * ScoreMultiplier;
            }
        }

            frameScores.Clear();    // Several scores can be collected in a frame, 
    }
    void SpawnEnemies()
    {
        if (!isSpawning) return;
        if (Time.time < nextSpawnTime) return;
        if (totalSpawned >= totalTargets) return;

        nextSpawnTime = Time.time + Random.Range(spawnIntervalMin, spawnIntervalMax);

        if (Target.Count >= maxEnemies) return; // We adjust the spawn time before checking this so new enemies dont snap in on killing an enemy.

        Target selectedTarget = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
        Vector3 spawnLocation = GetRandomLocationInBounds();

        Instantiate(selectedTarget, spawnLocation, Quaternion.identity, enemyHolder);
        totalSpawned++;

        if (totalSpawned >= totalTargets) isSpawning = false;
    }

    public void Score(int amount, GameObject hitObject)
    {
        score += amount;
        hitTargetsInFrame.Add(hitObject);
    }

    Vector3 GetRandomLocationInBounds()
    {
        return new Vector3(
            Random.Range(limits.Bounds.min.x, limits.Bounds.max.x),
            Random.Range(limits.Bounds.min.y, limits.Bounds.max.y),
            Random.Range(limits.Bounds.min.z, limits.Bounds.max.z)
            );
    }
    void StartGame()
    {
        if (isSpawning) return;
        score = 0;
        totalSpawned = 0;
        isSpawning = true;
        nextSpawnTime = Time.time + initialWaitTime;

    }
    void EndGame()
    {
        isSpawning = false;

        Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);

        foreach (Target target in targets) 
        {
            if (target == null) continue;
            Destroy(target.gameObject);
        }

    }
    #region Debug Input Methods
    private void DebugInputs()
    {
        KillTargetsInput();
        SpawnTargetsInput();
        StartCompetitiveInput();
        StartArcadeInput();
        EndGameInput();
    }
    void KillTargetsInput()
    {
        if (!killTargets.WasPressedThisFrame()) return;

        Debug.Log("Kill all targets");
        Target[] targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
        foreach (Target target in targets)
        {
            target.Hit();
        }

    }
    void SpawnTargetsInput()
    {
        if (!debugInputs.Debugging.SpawnTargets.WasPressedThisFrame()) return;
    }
    void StartCompetitiveInput()
    {
        if (debugInputs.Debugging.StartCompetitive.WasPressedThisFrame()) 
        {
            Debug.Log("Starting competitive");
            gameMode = GameMode.Competitive;
            StartGame();
        }

        if (debugInputs.Debugging.StartCompetitive.triggered)
        {
            Debug.Log("Starting competitive");
            gameMode = GameMode.Competitive;
            StartGame();
        }
    }
    void StartArcadeInput()
    {
        if (!debugInputs.Debugging.StartArcade.WasPressedThisFrame()) return;
        gameMode = GameMode.Arcade;
        StartGame();
    }

    void EndGameInput()
    {
        if (!debugInputs.Debugging.StopGame.WasPressedThisFrame()) return;

        EndGame();

    }
    #endregion

    #region Editor Methods
    private void OnValidate()
    {
        limits.Bounds = new Bounds
        {
            center = (limits.Min + limits.Max) * 0.5f + limits.Offset,
            size = Abs(limits.Max - limits.Min)
        };
    }
    private static Vector3 Abs(Vector3 v)
    {
        return new Vector3(
            Mathf.Abs(v.x),
            Mathf.Abs(v.y),
            Mathf.Abs(v.z)
        );
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(limits.Bounds.center, limits.Bounds.size);
    }
    #endregion
}
