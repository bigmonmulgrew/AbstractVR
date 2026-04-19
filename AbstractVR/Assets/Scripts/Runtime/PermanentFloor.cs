using UnityEngine;

public class PermanentFloor : MonoBehaviour
{
    static PermanentFloor instance;
    public static PermanentFloor Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<PermanentFloor>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("PermanentFloor");
                    instance = obj.AddComponent<PermanentFloor>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public static Transform Transform => Instance.transform;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
