using OpenCvSharp;
using OpenCvSharp.Demo;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Rect = UnityEngine.Rect;

public class BrightestSpotCalculator : MonoBehaviour
{
    public WebCamera webCamera;
    public RectangleDragger rectangleDragger;
    private BrightestSpotDetector brightestSpotDetector = new BrightestSpotDetector();
    private Rect rect;
    private Vector2 brightestSpot;

    public Vector2 BrightestSpot
    {
        get => brightestSpot;
        set => brightestSpot = value;
    }

    public Vector2 WorldPoint { get; set; }

    public Vector2 BrSpotRelToRect { get; set; }


    protected Rect CalculateDraggerRectInImageSpace()
    {
        var rectSp = webCamera.ConvertToImageSpace(rectangleDragger.StartPoint);
        rectSp.y = webCamera.imageMat.Size().Height - rectSp.y;
        var rectEp = webCamera.ConvertToImageSpace(rectangleDragger.EndPoint);
        rectEp.y = webCamera.imageMat.Size().Height - rectEp.y;

        return brightestSpotDetector.CalculateRect(rectSp, rectEp);
    }
    
    public bool IsPointInsideRectangle()
    {
        return rect.Contains(brightestSpot);
    }
    
    private void Update()
    {
        if (webCamera.imageMat == null) return;

        brightestSpot = brightestSpotDetector.BrightestSpotRelativeToCamera(webCamera.imageMat);
        WorldPoint = webCamera.ConvertToWorldSpace(brightestSpot);

        rect = CalculateDraggerRectInImageSpace();
        brightestSpot.y = webCamera.imageMat.Size().Height - brightestSpot.y;
        
        BrSpotRelToRect = brightestSpotDetector.BrightestSpotRelativeToRectangle(brightestSpot, rect);
    }
}
