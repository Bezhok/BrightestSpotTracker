namespace OpenCvSharp.Demo
{
	using System;
	using UnityEngine;
	using UnityEngine.UI;
	using OpenCvSharp;

	// Many ideas are taken from http://answers.unity3d.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html#answer-1155328

	/// <summary>
	/// Base WebCamera class that takes care about video capturing.
	/// Is intended to be sub-classed and partially overridden to get
	/// desired behavior in the user Unity script
	/// </summary>
	public class WebCamera: MonoBehaviour
	{
		/// <summary>
		/// Target surface to render WebCam stream
		/// </summary>
		public GameObject Surface;
		internal Mat imageMat;
		private Nullable<WebCamDevice> webCamDevice = null;
		private WebCamTexture webCamTexture = null;
		private Texture2D renderedTexture = null;
		protected const float downScale = 1f;
		
		/// <summary>
		/// A kind of workaround for macOS issue: MacBook doesn't state it's webcam as frontal
		/// </summary>
		protected bool forceFrontalCamera = false;
		internal RawImage rawImage;
		protected RectTransform rectTransform;

		/// <summary>
		/// WebCam texture parameters to compensate rotations, flips etc.
		/// </summary>
		protected Unity.TextureConversionParams TextureParameters { get; private set; }

		/// <summary>
		/// Camera device name, full list can be taken from WebCamTextures.devices enumerator
		/// </summary>
		public string DeviceName
		{
			get
			{
				return (webCamDevice != null) ? webCamDevice.Value.name : null;
			}
			set
			{
				// quick test
				if (value == DeviceName)
					return;

				if (null != webCamTexture && webCamTexture.isPlaying)
					webCamTexture.Stop();

				// get device index
				int cameraIndex = -1;
				for (int i = 0; i < WebCamTexture.devices.Length && -1 == cameraIndex; i++)
				{
					if (WebCamTexture.devices[i].name == value)
						cameraIndex = i;
				}

				// set device up
				if (-1 != cameraIndex)
				{
					webCamDevice = WebCamTexture.devices[0]; //cameraIndex
					
					webCamTexture = new WebCamTexture(webCamDevice.Value.name, 1280, 720, 30);
					// webCamTexture = new WebCamTexture(webCamDevice.Value.name, 3840, 2160, 60);
					// webCamTexture = new WebCamTexture(webCamDevice.Value.name, 1920, 1080, 60);
					
					// read device params and make conversion map
					ReadTextureConversionParameters();

					webCamTexture.Play();
				}
				else
				{
					throw new ArgumentException(String.Format("{0}: provided DeviceName is not correct device identifier", this.GetType().Name));
				}
			}
		}

		/// <summary>
		/// This method scans source device params (flip, rotation, front-camera status etc.) and
		/// prepares TextureConversionParameters that will compensate all that stuff for OpenCV
		/// </summary>
		private void ReadTextureConversionParameters()
		{
			Unity.TextureConversionParams parameters = new Unity.TextureConversionParams();

			// frontal camera - we must flip around Y axis to make it mirror-like
			parameters.FlipHorizontally = forceFrontalCamera || webCamDevice.Value.isFrontFacing;
			
			// TODO:
			// actually, code below should work, however, on our devices tests every device except iPad
			// returned "false", iPad said "true" but the texture wasn't actually flipped

			// compensate vertical flip
			//parameters.FlipVertically = webCamTexture.videoVerticallyMirrored;
			
			// deal with rotation
			if (0 != webCamTexture.videoRotationAngle)
				parameters.RotationAngle = webCamTexture.videoRotationAngle; // cw -> ccw

			// apply
			TextureParameters = parameters;

			//UnityEngine.Debug.Log (string.Format("front = {0}, vertMirrored = {1}, angle = {2}", webCamDevice.isFrontFacing, webCamTexture.videoVerticallyMirrored, webCamTexture.videoRotationAngle));
		}

		/// <summary>
		/// Default initializer for MonoBehavior sub-classes
		/// </summary>
		protected virtual void Awake()
		{
			rectTransform = Surface.GetComponent<RectTransform>();
			rawImage = Surface.GetComponent<RawImage>();
			if (WebCamTexture.devices.Length > 0)
				DeviceName = WebCamTexture.devices[WebCamTexture.devices.Length - 1].name;
		}

		void OnDestroy() 
		{
			if (webCamTexture != null)
			{
				if (webCamTexture.isPlaying)
				{
					webCamTexture.Stop();
				}
				webCamTexture = null;
			}

			if (webCamDevice != null) 
			{
				webCamDevice = null;
			}
		}

		/// <summary>
		/// Updates web camera texture
		/// </summary>
		private void Update()
		{
			if (webCamTexture != null && webCamTexture.didUpdateThisFrame)
			{
				// this must be called continuously
				ReadTextureConversionParameters();
				
				imageMat = Unity.TextureToMat(webCamTexture, TextureParameters);
				
				// process texture with whatever method sub-class might have in mind
				if (ProcessTexture(webCamTexture, ref renderedTexture))
				{
					
					RenderFrame();
				}
			}
		}

		/// <summary>
		/// Processes current texture
		/// This function is intended to be overridden by sub-classes
		/// </summary>
		/// <param name="input">Input WebCamTexture object</param>
		/// <param name="output">Output Texture2D object</param>
		/// <returns>True if anything has been processed, false if output didn't change</returns>
		protected  virtual bool ProcessTexture(WebCamTexture input, ref Texture2D output)
		{
			output = Unity.MatToTexture(imageMat, output);
			return true;
		}

		/// <summary>
		/// Renders frame onto the surface
		/// </summary>
		private void RenderFrame()
		{
			if (renderedTexture != null)
			{
				
				// apply
				rawImage.texture = renderedTexture;

				// Adjust image ration according to the texture sizes 
				rectTransform.sizeDelta = new Vector2(renderedTexture.width, renderedTexture.height);
			}
		}
		
		/// <summary>
		/// Converts point from screen space into the image space
		/// </summary>
		/// <param name="coord"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		protected Vector2 ConvertToImageSpace(Vector2 coord, Size size)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, coord, null, out var output);

			// pivot is in the center of the rectTransform, we need { 0, 0 } origin
			output.x += size.Width / 2;
			output.y += size.Height / 2;

			// now our image might have various transformations of it's own
			if (!TextureParameters.FlipVertically)
				output.y = size.Height - output.y;
			
			// downscaling
			output.x *= downScale;
			output.y *= downScale;

			return output;
		}

		public Vector2 ConvertToImageSpace(Vector2 coord)
		{
			return ConvertToImageSpace(coord, imageMat.Size());
		}

		public static UnityEngine.Rect RectTransformToScreenSpace(RectTransform transform)
		{
			Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
			return new UnityEngine.Rect((Vector2)transform.position - (size * transform.pivot), size);
		}

		internal Vector2 ConvertToWorldSpace(Vector2 coord)
		{
			return ConvertToWorldSpace(coord, imageMat.Size());
		}
		
		internal Vector2 ConvertToWorldSpace(Vector2 coord, Size size)
		{
			Vector2 output = coord;

			// var rectTransform = rawImage.rectTransform;

			// now our image might have various transformations of it's own
			if (!TextureParameters.FlipVertically)
				output.y = size.Height - output.y;
			
			Vector2 posRelativeToRectScaled = Vector2.Scale(output, rectTransform.lossyScale);
			output = posRelativeToRectScaled;

			// upscaling
			output.x /= downScale;
			output.y /= downScale;

			var rect = RectTransformToScreenSpace(rectTransform);
			output += new Vector2(rect.xMin, rect.yMin);
			
			return output;
		}
	}
}