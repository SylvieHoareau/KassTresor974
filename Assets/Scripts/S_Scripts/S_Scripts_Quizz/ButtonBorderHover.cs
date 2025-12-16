using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonBorderHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public Image borderImage;
    public Color normalColor;
    public Color hoverColor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        borderImage.color = hoverColor;
        transform.localScale = Vector3.one * 1.05f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        borderImage.color = normalColor;
        transform.localScale = Vector3.one;
    }

}
