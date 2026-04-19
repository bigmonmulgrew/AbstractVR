using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Assign unity engine random to Random to avoid ambiguity with System.Random
using Random = UnityEngine.Random;

public sealed class EffectWrapper : MonoBehaviour
{
    [Header("Effect Prefabs")]
    [SerializeField] private List<GameObject> particleEffectPrefabs = new();
    [SerializeField] private List<GameObject> visualEffectPrefabs = new();

    [Header("Pooling")]
    [Min(1)]
    [SerializeField] private int minimumPoolSize = 3;

    public event Action OnEffectFinished;

    private readonly HashSet<PooledEffectInstance> nowPlaying = new();
    private readonly List<GameObject> allPrefabs = new();

    private static readonly Dictionary<GameObject, PoolEntry> s_PoolsByPrefab = new();
    private static Transform s_PoolRoot;

    GameObject RandomEffectPrefab 
    {        
        get
        {
            if(allPrefabs == null || allPrefabs.Count == 0) return null;

            int index = Random.Range(0, allPrefabs.Count);

            return allPrefabs[index];
        }
    }
    private sealed class PoolEntry
    {
        public ObjectPool<PooledEffectInstance> Pool;
        public readonly HashSet<int> Owners = new();
        public int RequiredMinSize;
        public int CreatedCount;
        public bool IsPrewarming;
    }

    private void Awake()
    {
        EnsurePoolRoot();

        allPrefabs.AddRange(particleEffectPrefabs);
        allPrefabs.AddRange(visualEffectPrefabs);

        if (allPrefabs.Count == 0)
        {
            Debug.LogWarning($"{nameof(EffectWrapper)} on '{name}' has no effect prefabs. Destroying wrapper.", this);
            Destroy(this);
            return;
        }

        int ownerId = GetInstanceID();

        foreach (GameObject prefab in allPrefabs)
            RegisterPrefab(prefab, ownerId, minimumPoolSize);
    }

    private void OnDestroy()
    {
        StopInternal(clearPlayingSet: true);

        if (allPrefabs.Count == 0) return;

        int ownerId = GetInstanceID();

        foreach (GameObject prefab in allPrefabs)
            UnregisterPrefab(prefab, ownerId);
    }
    
    public void Play(int n = 1)
    {       
        if (n <= 0) return;
 
        if (allPrefabs.Count == 0) return;

        for(int i = 0; i < n; i++)
            PlayOne();
    }
    void PlayOne()
    {
        GameObject prefab = RandomEffectPrefab;
        if (prefab == null) return;

        if (!s_PoolsByPrefab.TryGetValue(prefab, out PoolEntry entry))
        {
            RegisterPrefab(prefab, GetInstanceID(), minimumPoolSize);

            if (!s_PoolsByPrefab.TryGetValue(prefab, out entry)) return;

        }

        PooledEffectInstance instance = entry.Pool.Get();
        instance.transform.SetParent(transform, false);

        instance.Finished -= HandleEffectInstanceFinished;
        instance.Finished += HandleEffectInstanceFinished;

        nowPlaying.Add(instance);
        instance.Play();
    }

    public void Stop()
    {
        StopInternal(clearPlayingSet: true);
    }

    public void Reset()
    {
        StopInternal(clearPlayingSet: true);
    }

    private void StopInternal(bool clearPlayingSet)
    {
        if (nowPlaying.Count == 0)
            return;

        var buffer = new List<PooledEffectInstance>(nowPlaying);

        foreach (PooledEffectInstance instance in buffer)
        {
            if (instance == null)
                continue;

            instance.Finished -= HandleEffectInstanceFinished;
            instance.StopImmediate();
            instance.ReturnToPool();
        }

        if (clearPlayingSet)
            nowPlaying.Clear();
    }

    private void HandleEffectInstanceFinished(PooledEffectInstance instance)
    {
        if (instance == null) return;

        instance.Finished -= HandleEffectInstanceFinished;

        bool removed = nowPlaying.Remove(instance);
        instance.ReturnToPool();

        if (removed && nowPlaying.Count == 0) OnEffectFinished?.Invoke();
    }

    private static void RegisterPrefab(GameObject prefab, int ownerId, int requestedMinPoolSize)
    {
        if (prefab == null) return;

        EnsurePoolRoot();

        if (!s_PoolsByPrefab.TryGetValue(prefab, out PoolEntry entry))
        {
            entry = new PoolEntry();
            entry.RequiredMinSize = Mathf.Max(1, requestedMinPoolSize);

            entry.Pool = new ObjectPool<PooledEffectInstance>(
                createFunc: () => { return CreateItem(prefab, entry); },
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyItem,
                collectionCheck: true,
                defaultCapacity: entry.RequiredMinSize,
                maxSize: Mathf.Max(entry.RequiredMinSize * 4, entry.RequiredMinSize)
            );

            s_PoolsByPrefab.Add(prefab, entry);
        }

        entry.Owners.Add(ownerId);

        int newRequiredMin = Mathf.Max(entry.RequiredMinSize, Mathf.Max(1, requestedMinPoolSize));
        if (newRequiredMin > entry.RequiredMinSize) entry.RequiredMinSize = newRequiredMin;

        Prewarm(entry);
    }

    static PooledEffectInstance CreateItem(GameObject prefab, PoolEntry entry)
    {
        GameObject go = Instantiate(prefab, s_PoolRoot);
        go.name = $"{prefab.name} (Pooled)";

        if (!go.TryGetComponent<PooledEffectInstance>(out var instance))
        {
            Debug.LogError(
                $"Pooled effect prefab '{prefab.name}' is missing a {nameof(PooledEffectInstance)} component.",
                prefab);

            Destroy(go);
            return null;
        }

        go.SetActive(false);
        instance.Bind(prefab, entry.Pool);

        if (!entry.IsPrewarming)
        {
            Debug.Log(
                $"Effect pool for '{prefab.name}' expanded beyond its prewarmed minimum of {entry.RequiredMinSize}.",
                prefab);
        }

        entry.CreatedCount++;
        return instance;
    }

    static void OnGet(PooledEffectInstance instance)
    {
        if (instance == null) return;
        instance.gameObject.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

    }
    static void OnRelease(PooledEffectInstance instance)
    {
        if (instance == null) return;

        instance.transform.SetParent(s_PoolRoot, false);
        instance.gameObject.SetActive(false);
    }
    static void OnDestroyItem(PooledEffectInstance instance)
    {
        if (instance != null) Destroy(instance.gameObject);
    }
    
    private static void UnregisterPrefab(GameObject prefab, int ownerId)
    {
        if (prefab == null) return;

        if (!s_PoolsByPrefab.TryGetValue(prefab, out PoolEntry entry)) return;

        entry.Owners.Remove(ownerId);

        if (entry.Owners.Count > 0) return;

        entry.Pool.Clear();
        s_PoolsByPrefab.Remove(prefab);
    }

    private static void Prewarm(PoolEntry entry)
    {
        if (entry == null || entry.Pool == null) return;

        if (entry.CreatedCount >= entry.RequiredMinSize) return;

        entry.IsPrewarming = true;
        List<PooledEffectInstance> temp = new();

        try
        {
            while (entry.CreatedCount < entry.RequiredMinSize)
            {
                PooledEffectInstance instance = entry.Pool.Get( );
                if (instance != null) temp.Add(instance);
            }

            for (int i = 0; i < temp.Count; i++)
                entry.Pool.Release(temp[i]);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
        finally
        {
            entry.IsPrewarming = false;
            temp.Clear();
        }
    }

    private static void EnsurePoolRoot()
    {
        if (s_PoolRoot != null) return;

        GameObject root = new GameObject("__EffectWrapperPools");
        DontDestroyOnLoad(root);
        s_PoolRoot = root.transform;
    }


}