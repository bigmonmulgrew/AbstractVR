using UnityEngine;

public class HeadLookTracker : MonoBehaviour
{
    [SerializeField] private Transform centerEyeAnchor;

    void Update()
    {
        Vector3 lookOrigin = centerEyeAnchor.position;
        Vector3 lookDirection = centerEyeAnchor.forward;

        Debug.DrawRay(lookOrigin, lookDirection * 5f, Color.red);

        if (Physics.Raycast(lookOrigin, lookDirection, out RaycastHit hit, 100f))
        {
            Debug.Log("Looking at: " + hit.collider.name);
        }
    }
}