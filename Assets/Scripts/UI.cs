using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UI : MonoBehaviour
{
    public TMP_Text positionText;
    public UICircle circle;
    private BrightestSpotCalculator spotCalc;

    private void Start()
    {
        spotCalc = GetComponent<BrightestSpotCalculator>();
    }
    
    private void Update()
    {
        string message = $"({spotCalc.BrightestSpot.x}, {spotCalc.BrightestSpot.y}) ";
        
        if (spotCalc.IsPointInsideRectangle()) message += $"({(int)spotCalc.BrSpotRelToRect.x}, {(int)spotCalc.BrSpotRelToRect.y})";
        else message += "(0, 0)";

        message += " - relative to rectangle";
        positionText.SetText(message);

        circle.rectTransform.position = spotCalc.WorldPoint;
    }    
}