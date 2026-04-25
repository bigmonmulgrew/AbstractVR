using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShootUI : MonoBehaviour
{
    public static readonly LayerMask HIT_LAYERS = 4096; //  Debug.Log(LayerMask.GetMask("ShootUI"));
    [SerializeField] UnityEvent onShoot;

    Image background;
    Color bgColour;
    private void Awake()
    {
        background = GetComponentInChildren<Image>();
        bgColour = background.color;
    }

    IEnumerator ReactoOnShoot()
    {

        yield return new WaitForSeconds(0.1f);
    }

    public void Hit()
    {
        onShoot?.Invoke();

    }
}
