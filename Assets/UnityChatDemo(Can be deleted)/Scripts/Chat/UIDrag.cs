using UnityEngine;
using UnityEngine.EventSystems;


public class UIDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
{

    private float offsetX;
    private float offsetY;

    public Transform Transform;

    Vector2 startPosition;
    public void Awake()
    {
        if (Transform == null)
            Transform = transform;

        startPosition = Transform.GetComponent<RectTransform>().anchoredPosition;
    }

    void OnEnable()
    {
        Transform.GetComponent<RectTransform>().anchoredPosition = startPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offsetX = Transform.position.x - Input.mousePosition.x;
        offsetY = Transform.position.y - Input.mousePosition.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Transform.position = new Vector3(offsetX + Input.mousePosition.x, offsetY + Input.mousePosition.y, 0);
    }

}
