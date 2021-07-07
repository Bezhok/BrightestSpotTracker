using OpenCvSharp;
using UnityEngine;
using Rect = UnityEngine.Rect;

public class BrightestSpotDetector
{
    public Color32 lowerRGBBound;
    public Color32 upperRGBBound;
    
    public Vector2 BrightestSpotRelativeToCamera(Mat imageMat)
    {
        Cv2.MinMaxLoc(TransformedImage(imageMat), out double minVal, out double maxVal, out Point minLoc, out Point maxLoc);
        
        return new Vector2(maxLoc.X, maxLoc.Y);
    }

    public Mat TransformedImage(Mat imageMat)
    {
        int radius = 11;
        
        // // without mask
        // Mat imageGray = new Mat();
        // Cv2.CvtColor(image, imageGray, ColorConversionCodes.BGR2GRAY);
        // Cv2.GaussianBlur(imageGray, imageGray, new Size(radius, radius), 0);
        // return imageGray;

        Mat imgCopy = new Mat();
        imageMat.CopyTo(imgCopy);
        
        Mat hsvMask = new Mat();
        imageMat.CopyTo(hsvMask);
        
        Cv2.GaussianBlur(hsvMask, hsvMask, new Size(radius, radius), 0);
        Cv2.CvtColor(hsvMask,  hsvMask, ColorConversionCodes.BGR2HSV);
        
        // depends on laser color
        var lower = new Scalar(lowerRGBBound.b, lowerRGBBound.g, lowerRGBBound.r);
        var upper = new Scalar(upperRGBBound.b, upperRGBBound.g, upperRGBBound.r);

        Cv2.InRange(hsvMask, lower, upper, hsvMask);
        Cv2.Erode(hsvMask, hsvMask, null, null, 2);
        Cv2.Dilate(hsvMask, hsvMask, null, null, 2);
        
        Cv2.BitwiseNot(imgCopy, imgCopy);
        Cv2.BitwiseNot(hsvMask, hsvMask);
        // for some reason if src1==src2 mask doesnt work
        Cv2.BitwiseOr(imgCopy, imageMat, imgCopy, hsvMask);
        Cv2.BitwiseNot(imgCopy, imgCopy);
			
        Cv2.GaussianBlur(imgCopy, imgCopy, new Size(radius, radius), 0);
        Cv2.CvtColor(imgCopy, imgCopy, ColorConversionCodes.BGR2GRAY);

        return imgCopy;
    }
    
    public Vector2 BrightestSpotRelativeToRectangle(Vector2 p, Rect rect)
    {
        return new Vector2(p.x - rect.xMin,  p.y - rect.yMin);
    }
    
    public Rect CalculateRect(Vector2 rectSp, Vector2 rectEp)
    {
        return new Rect(
            Mathf.Min(rectSp.x, rectEp.x), Mathf.Min(rectSp.y, rectEp.y),
            Mathf.Abs(rectEp.x-rectSp.x), Mathf.Abs(rectEp.y-rectSp.y)
        );
    }
}