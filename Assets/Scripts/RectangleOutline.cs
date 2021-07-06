using System;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class RectangleOutline : MonoBehaviour
{
    private UILineRenderer uiLineRenderer;
    
    // relative to image
    public Vector2 StartPoint { get; set; } = Vector2.zero;
    public Vector2 EndPoint { get; set; } = Vector2.zero;

    private Vector2 position;
    private Vector2 size;

    private void Start()
    {
        uiLineRenderer = GetComponent<UILineRenderer>();
    }

    private Rect UpdatedRect(Vector2 sp, Vector2 ep)
    {
        position = new Vector2(Mathf.Min(sp.x, ep.x), Mathf.Min(sp.y, ep.y));
        size = new Vector2(Mathf.Abs(ep.x - sp.x), Mathf.Abs(ep.y - sp.y));
        
        return new Rect(position, size);
    }

    private void UpdateLine(Rect rect)
    {
        var pointList = new Vector2[5];
        pointList[0] = rect.min;
        pointList[1] = rect.min+new Vector2(rect.width, 0);
        pointList[2] = rect.max;
        pointList[3] = rect.min+new Vector2(0, rect.height);
        pointList[4] = rect.min;

        uiLineRenderer.Points = pointList;
    }

    private void Update()
    {
        var rect = UpdatedRect(StartPoint, EndPoint);
        UpdateLine(rect);
    }
}
