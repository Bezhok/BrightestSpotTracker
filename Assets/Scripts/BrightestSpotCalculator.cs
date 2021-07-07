using System;
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
    internal BrightestSpotDetector brightestSpotDetector = new BrightestSpotDetector();
    private Rect rect;
    private Vector2 brightestSpot;
    public Color32 lowerRGBBound;
    public Color32 upperRGBBound;
    
    public Vector2 BrightestSpot
    {
        get => brightestSpot;
        set => brightestSpot = value;
    }

    public Vector2 WorldPoint { get; set; }

    public Vector2 BrSpotRelToRect { get; set; }


    private void Start()
    {
        lowerRGBBound = new Color32(0,25,155, 255);
        upperRGBBound = new Color32(255,255,179, 255);
    }

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

        brightestSpotDetector.lowerRGBBound = lowerRGBBound;
        brightestSpotDetector.upperRGBBound = upperRGBBound;
        
        brightestSpot = brightestSpotDetector.BrightestSpotRelativeToCamera(webCamera.imageMat);
        WorldPoint = webCamera.ConvertToWorldSpace(brightestSpot);

        rect = CalculateDraggerRectInImageSpace();
        brightestSpot.y = webCamera.imageMat.Size().Height - brightestSpot.y;
        
        BrSpotRelToRect = brightestSpotDetector.BrightestSpotRelativeToRectangle(brightestSpot, rect);
    }
}
