using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    const string bootstrap = "Bootstrap";
    const string loadingSceneName = "LoadingScene";

    const string Level1 = "Level1";
    const string debug1 = "SampleScene";
    const string debug2 = "BasicScene";
    const string debug3 = "DemoScene";
    const string debug4 = "HandsDemoScene";
    const string debug5 = "HandVisualizer";

    [SerializeField] private Transform xrOriginRoot;

    XRIDefaultInputActions inputActions;
    string currentGameplayScene;

    bool isTransitioning = false;

    private void Awake()
    {
        CreateInstance();
        if (Instance != this) return;

        inputActions = new XRIDefaultInputActions();
        
    }
    private void Start()
    {
        LoadScene(Level1);
    }

    private void CreateInstance()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    static void LoadScene(string name)
    {
       if (!Instance.isTransitioning)
       {
           Instance.StartCoroutine(Instance.LoadLevelRoutine(name));
       }
    }

    IEnumerator LoadLevelRoutine(string nextSceneName)
    {
        isTransitioning = true;

        // 1. Load loading scene additively
        yield return SceneManager.LoadSceneAsync(loadingSceneName, LoadSceneMode.Additive);

        Scene loadingScene = SceneManager.GetSceneByName(loadingSceneName);
        SceneManager.SetActiveScene(loadingScene);

        LoadingFogController fog = FindFirstObjectByType<LoadingFogController>();
        if (fog != null) yield return fog.FogIn();

        // 2. Load destination additively
        yield return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        LightProbes.Tetrahedralize();

        Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
        SceneManager.SetActiveScene(nextScene);

        // 3. Move player to spawn if needed
        Transform spawn = FindFirstSpawnInScene(nextScene);
        if (spawn != null && xrOriginRoot != null) xrOriginRoot.position = spawn.position;

        // 4. Unload old gameplay scene
        if (!string.IsNullOrEmpty(currentGameplayScene) && currentGameplayScene != nextSceneName)
            yield return SceneManager.UnloadSceneAsync(currentGameplayScene);

        currentGameplayScene = nextSceneName;

        // 5. Reveal new scene
        if (fog != null) yield return fog.FogOut();

        // 6. Remove loading scene
        yield return SceneManager.UnloadSceneAsync(loadingSceneName);

        isTransitioning = false;
    }

    private Transform FindFirstSpawnInScene(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var spawn = root.GetComponentInChildren<PlayerSpawnPoint>();
            if (spawn != null) return spawn.transform;
        }
        return null;
    }

    #region Debug level loading
    private void Update()
    {
        if (inputActions.Debugging.LoadLevel1.triggered) LoadDebug1();
        if (inputActions.Debugging.LoadLevel2.triggered) LoadDebug2();
        if (inputActions.Debugging.LoadLevel3.triggered) LoadDebug3();
        if (inputActions.Debugging.LoadLevel4.triggered) LoadDebug4();
        if (inputActions.Debugging.LoadLevel5.triggered) LoadDebug5();
    }
    public void LoadDebug1() => LoadScene(debug1);
    public void LoadDebug2() => LoadScene(debug2);
    public void LoadDebug3() => LoadScene(debug3);
    public void LoadDebug4() => LoadScene(debug4);
    public void LoadDebug5() => LoadScene(debug5);

    #endregion
}
