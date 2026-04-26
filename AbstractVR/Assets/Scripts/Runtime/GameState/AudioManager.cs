using UnityEngine;

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;
    static AudioSource source;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<AudioManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                    source = obj.AddComponent<AudioSource>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }
    private void Awake()
    {
        CreateInstance();
        SetRelaxedMusic();
    }

    public static void SetRelaxedMusic()
    {
        source.pitch = 0.8f;
    }

    public static void SetTenseMusic()
    {
        source.pitch = 1.0f;
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
            source = GetComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
    }
    public static void QuietMode(bool enabled = true)
    {
        if (enabled) source.volume = 0.1f;
        else source.volume = 1.0f;
    }
}
