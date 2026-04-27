using System.Collections.Generic;
using UnityEngine;

public class TouchChain : MonoBehaviour
{
    [SerializeField] List<TouchOrb> orbs = new();
    [SerializeField] bool startOnEnable = true;
    [SerializeField] float distanceBelowHead = 0.5f;

    static GameObject hand;

    int currentIndex;
    int totalScore;
    bool running;

    static bool alreadyRunning = false;

    void Awake()
    {
        if (alreadyRunning)
        {
            Destroy(this.gameObject);
            return;
        }
        alreadyRunning = true;
        if (!GameManager.Instance.IsArcadeMode)
        {
            Destroy(this.gameObject);
            return;
        }

        for (int i = 0; i < orbs.Count; i++)
        {
            orbs[i].Initialize(this, i);
            orbs[i].SetWaiting();
        }
        transform.parent = null;
    }
    void SetupStartPosition()
    {
        FindRightHand();

        if(hand == null)
        {
            Debug.Log("OVR controller helper hand not found");
            transform.position = Vector3.zero;
            return;
        }

        // Start at mean position X and Z for hand and head
        // Make start a short amount lower than head
        Vector3 headPos = AimingReticle.Instance.HeadPosition;
        Vector3 handPos = AimingReticle.Instance.HandPosition;
        Vector3 startingPos = new(
            (headPos.x + handPos.x) * 0.5f,
            headPos.y - distanceBelowHead,
            (headPos.z + handPos.z) * 0.5f
            );

        transform.position = startingPos;
        transform.parent = Player.Instance.transform;
    }

    void FindRightHand()
    {
        if (hand != null) return;

        OVRCameraRig rig = Player.Instance.GetComponentInChildren<OVRCameraRig>(true);

        if (rig != null && rig.rightHandAnchor != null)
        {
            hand = CreateHandTriggerProxy(rig.rightHandAnchor);
        }
    }
    GameObject CreateHandTriggerProxy(Transform parent)
    {
        GameObject proxy = new GameObject("RuntimeRightHandTriggerProxy");
        proxy.transform.SetParent(parent, false);
        proxy.transform.localPosition = Vector3.zero;
        proxy.transform.localRotation = Quaternion.identity;
        proxy.transform.localScale = Vector3.one;

        proxy.layer = LayerMask.NameToLayer("OVRInterraction");

        SphereCollider sphere = proxy.AddComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 0.08f;

        Rigidbody rb = proxy.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        return proxy;
    }

    void OnEnable()
    {
        // Allows auto restart
        if (startOnEnable) StartChain();
    }

    public void StartChain()
    {
        SetupStartPosition();
        currentIndex = 0;
        totalScore = 0;
        running = true;

        foreach (TouchOrb orb in orbs)
        {
            orb.SetWaiting();
        }

        if (orbs.Count > 0)
        {
            orbs[0].Activate();
        }
    }

    public void OrbTouched(TouchOrb orb, int orbIndex, int awardedScore)
    {
        if (!running) return;
        if (orbIndex != currentIndex) return;

        totalScore += awardedScore;

        orb.Complete();

        currentIndex++;

        if (currentIndex >= orbs.Count)
        {
            CompleteChain();
            return;
        }

        orbs[currentIndex].Activate();
    }

    public void OrbExpired(TouchOrb orb, int orbIndex)
    {
        if (!running) return;

        if (orbIndex != currentIndex) return;

        FailChain();
        Destroy(this.gameObject);
        alreadyRunning = false;
    }

    void CompleteChain()
    {
        running = false;
        Debug.Log($"TouchChain complete. Score: {totalScore}");
        GameManager.Instance.Score(totalScore);
        alreadyRunning = false;
        Destroy(gameObject);
    }

    void FailChain()
    {
        running = false;
        Debug.Log($"TouchChain failed. Score: {totalScore}");
    }
}