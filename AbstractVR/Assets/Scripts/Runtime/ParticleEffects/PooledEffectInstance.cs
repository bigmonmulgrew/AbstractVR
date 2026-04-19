using System;
using UnityEngine;
using UnityEngine.Pool;

public abstract class PooledEffectInstance : MonoBehaviour
{
    public event Action<PooledEffectInstance> Finished;

    private ObjectPool<PooledEffectInstance> owningPool;
    private bool isReleasing;

    public GameObject SourcePrefab { get; private set; }

    public void Bind(GameObject sourcePrefab, ObjectPool<PooledEffectInstance> pool)
    {
        SourcePrefab = sourcePrefab;
        owningPool = pool;
    }

    public abstract void Play();
    public abstract void StopImmediate();
    public abstract void ResetState();
    protected void RaiseFinished()
    {
        if (isReleasing) return;

        Finished?.Invoke(this);
    }
    public void ReturnToPool()
    {
        if (isReleasing) return;

        isReleasing = true;

        try
        {
            ResetState();

            if (owningPool != null) owningPool.Release(this);
            else                    gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
        }
        finally
        {
            isReleasing = false;
        }
    }

}