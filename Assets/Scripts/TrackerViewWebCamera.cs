using OpenCvSharp;
using OpenCvSharp.Demo;
using UnityEngine;

public class TrackerViewWebCamera: WebCamera
{
    public bool trackerView;
    public BrightestSpotCalculator brightestSpotCalculator;
    
    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        if (!trackerView)
        {
            base.ProcessTexture(input, ref output);
            return true;
        }

        output = OpenCvSharp.Unity.MatToTexture(
            brightestSpotCalculator.brightestSpotDetector.TransformedImage(imageMat),
            output
            );
        
        return true;
    }
}