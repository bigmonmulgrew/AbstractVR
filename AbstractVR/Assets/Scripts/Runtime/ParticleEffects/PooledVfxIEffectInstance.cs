using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public sealed class PooledVfxEffectInstance : PooledEffectInstance
{
    [SerializeField] string finishedEventName = "OnFinished";

    VisualEffect visualEffect;
    int finishedEventId;

    private bool sawAliveParticles;
    private bool waitingForFallbackFinish;
    private float zeroAliveSince = -1f;

    private void Awake()
    {
        visualEffect = GetComponent<VisualEffect>();
        finishedEventId = Shader.PropertyToID(finishedEventName);
        visualEffect.outputEventReceived += OnOutputEventReceived;
    }

    private void OnDestroy()
    {
        if (visualEffect != null) visualEffect.outputEventReceived -= OnOutputEventReceived;
    }

    private void Update()
    {
        if (!waitingForFallbackFinish) return;

        uint alive = (uint)visualEffect.aliveParticleCount;

        if (alive > 0)
        {
            sawAliveParticles = true;
            zeroAliveSince = -1f;
            return;
        }

        if (!sawAliveParticles) return;

        if (zeroAliveSince < 0f) zeroAliveSince = Time.time;

        if (Time.time - zeroAliveSince > 0.05f)
        {
            waitingForFallbackFinish = false;
            RaiseFinished();
        }
    }

    public override void Play()
    {
        sawAliveParticles = false;
        waitingForFallbackFinish = true;
        zeroAliveSince = -1f;

        visualEffect.Reinit();
        visualEffect.Play();
    }

    public override void StopImmediate()
    {
        visualEffect.Stop();
        visualEffect.Reinit();
    }

    public override void ResetState()
    {
        visualEffect.Stop();
        visualEffect.Reinit();
    }

    private void OnOutputEventReceived(VFXOutputEventArgs args)
    {
        if (args.nameId == finishedEventId) RaiseFinished();
    }
}