using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ShootUI : MonoBehaviour
{
    public static readonly LayerMask HIT_LAYERS = 4096; //  Debug.Log(LayerMask.GetMask("ShootUI"));
    [SerializeField] UnityEvent onShoot;
    [SerializeField] float interactTime = 0.2f;


    Image background;
    Color bgColour;
    TextMeshProUGUI textElement;
    Color textColour;

    protected bool isActive;

    private void Awake()
    {
        background = GetComponentInChildren<Image>();
        if (background != null ) bgColour = background.color;
        
        textElement = GetComponentInChildren<TextMeshProUGUI>();
        if (textElement != null ) textColour = textElement.color;
    }

    IEnumerator ReactoOnShoot()
    {
        Color swapBgColour = new(
            1 - bgColour.r,
            1 - bgColour.g,
            1 - bgColour.b,
            1.0f
            );
        Color swapTextColour = new(
            1 - textColour.r,
            1 - textColour.g,
            1 - textColour.b,
            1.0f
            );

        if (background)  background.color = swapBgColour;
        if (textElement) textElement.color = swapTextColour;

        yield return new WaitForSecondsRealtime(interactTime);

        if (background)  background.color = bgColour;
        if (textElement) textElement.color = textColour;

        onShoot?.Invoke();
        isActive = false;
    }

    public virtual void Hit()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(ReactoOnShoot());
    }
}
