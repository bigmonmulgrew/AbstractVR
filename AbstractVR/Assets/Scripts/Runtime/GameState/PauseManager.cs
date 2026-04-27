using System;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    bool isPaused = false;

    public event Action OnGamePaused;
    public event Action OnGameUnpaused;

    public bool IsGamePaused => isPaused;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        SetPaused(pauseStatus);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        SetPaused(!hasFocus);
    }

    public void SetPaused(bool paused)
    {
        if (isPaused == paused) return;

        isPaused = paused;

        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

        Debug.Log(paused ? "Game paused" : "Game resumed");

        if (paused) OnGamePaused?.Invoke();
        else        OnGameUnpaused?.Invoke();
    }

}
