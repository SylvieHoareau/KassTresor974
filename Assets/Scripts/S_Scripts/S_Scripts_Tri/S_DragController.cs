using NUnit.Framework;
using UnityEngine;

public class DragController : MonoBehaviour
{
    public S_Draggable lastDragged => _lastDragged;

    private bool _isDragActive = false;

    private Vector2 _screenPosition;

    private Vector3 _worldPosition;

    private S_Draggable _lastDragged;

    void Awake()
    {
         DragController[] controllers = FindObjectsByType<DragController>(FindObjectsSortMode.None);
        if (controllers.Length > 1)
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDragActive && (Input.GetMouseButtonUp(0) || Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended))
        {
            Drop();
            return;
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            _screenPosition = new Vector2(mousePosition.x, mousePosition.y);
        }
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            _screenPosition = new Vector2(touch.position.x, touch.position.y);
        }
        else{
            return;
        }

        _worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(_screenPosition.x, _screenPosition.y, Camera.main.nearClipPlane));

        if (_isDragActive)
        {
            Drag();
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(_worldPosition, Vector2.zero);
            if (hit.collider != null)
            {
                S_Draggable draggable = hit.collider.GetComponent<S_Draggable>();
                if (draggable != null)
                {
                    _lastDragged = draggable;
                    InitDrag();
                }
            }
        }
    }

    public void InitDrag() 
    {
        // _lastDragged.LastPosition = _lastDragged.transform.position;
        UpdateDragStatus(true);
    }

    public void Drag()
    {
        _lastDragged.transform.position = new Vector2(_worldPosition.x, _worldPosition.y);
    }

    public void Drop()
    {
        UpdateDragStatus(false);
    }

    public void UpdateDragStatus(bool IsDragging)
    {
        _isDragActive = IsDragging;
        // _lastDragged.IsDragging = IsDragging;
        _lastDragged.gameObject.layer = IsDragging ? Layer.Dragging : Layer.Default;
        // if (IsDragging)
        // {
        //     gameObject.layer = Layer.Dragging;
        // }
        // else
        // {
        //     gameObject.layer = Layer.Default;
        // }
    }
}
