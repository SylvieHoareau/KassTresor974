using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.EventSystems;

public class S_Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identifiant unique de la carte")]
    public int id;

    private CanvasGroup canvasGroup;
    private Canvas canvas;
    public RectTransform rectTransform;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Vector2 originalAnchoredPos;

    public S_DropSlot current_slot;
    private Vector3 startPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();

        // On stocke la position et le parent d'origine
        startPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        originalAnchoredPos = rectTransform.anchoredPosition;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if(current_slot != null)
        {
            current_slot.cardInSlot = null;
            current_slot = null;
        }

        // On place la carte au-dessus du canvas pour qu'elle soit par-dessus
        // transform.SetParent(canvas.transform);

        // Permet aux slots de détecter le drop
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        // transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // La carte peut être déposé sur un slot
        canvasGroup.blocksRaycasts = true;

        // Si la carte n'a PAS été placée dans un slot
        if (current_slot == null)
        {
            ResetPos();
        }
        // Si la carte est dans un slot, l'ancrer au centre
        else
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void ResetPos()
    {
        transform.parent = originalParent;
        rectTransform.anchoredPosition = startPosition;
        current_slot = null;
    }

}
