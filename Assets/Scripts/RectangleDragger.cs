using OpenCvSharp.Demo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RectangleDragger : MonoBehaviour
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{
    public RectangleOutline rectangleOutline;
    
    public Vector2 StartPoint { get; set; }
    public Vector2 EndPoint { get; set; }
    
    private bool isDragging = false;
    private RawImage rawImage;

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        UpdateRectanglOutline();
    }

    private void UpdateRectanglOutline()
    {
        Vector2 sp = StartPoint;
        Vector2 ep = EndPoint;
        
        if (rawImage == null) return;
        
        // screen space to rectangle outline space
        var rect = WebCamera.RectTransformToScreenSpace(rawImage.rectTransform);
        // TODO pivot center
        sp -= new Vector2(rect.xMin+rect.width/2, rect.yMin+rect.height/2);
        ep -= new Vector2(rect.xMin+rect.width/2, rect.yMin+rect.height/2);
        
        rectangleOutline.StartPoint = sp;
        rectangleOutline.EndPoint = ep;
    }
    
    private void DropTracking()
    {
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        DropTracking();

        isDragging = true;
        StartPoint = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        EndPoint = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndPoint = eventData.position;
        isDragging = false;
    }
}