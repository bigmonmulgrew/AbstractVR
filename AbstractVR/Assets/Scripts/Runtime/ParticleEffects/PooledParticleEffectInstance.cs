using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public sealed class PooledParticleEffectInstance : PooledEffectInstance
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();


        var main = ps.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Callback;

    }

    public override void Play()
    {
        ps.Clear(true);
        ps.Play(true);
    }

    public override void StopImmediate()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public override void ResetState()
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);
    }

    private void OnParticleSystemStopped()
    {
        RaiseFinished();
    }
}