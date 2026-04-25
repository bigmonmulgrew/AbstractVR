using UnityEngine;

public class ShootUI : MonoBehaviour
{
    public static readonly LayerMask HIT_LAYERS = 4096; //  Debug.Log(LayerMask.GetMask("ShootUI"));

    public void Hit()
    {
        Debug.LogError($"ShootUI: Hit {name}, activation not implemented");
        
    }
}
