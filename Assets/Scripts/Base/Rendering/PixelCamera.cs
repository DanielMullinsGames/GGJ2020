using UnityEngine;
using System.Collections;

public class PixelCamera : MonoBehaviour 
{
    private const int REFERENCE_HEIGHT_MIN = 450;
    private const int REFERENCE_HEIGHT_MAX = 550;

    private const float MIN_ASPECT_RATIO = 1.75f;

	public const int PIXELS_PER_UNIT = 100;

    [SerializeField]
	private int referenceHeight;

	private int renderWidth;
	private int actualWidth;
	private int actualHeight;
	
	private Camera cam;

	void Awake ()
    {
		cam = GetComponent<Camera>();
	}

	void Update()
    {
		cam.orthographicSize = (referenceHeight / 2) / (float)PIXELS_PER_UNIT;

        int scale = Mathf.Max(1, Mathf.Max(Screen.height / referenceHeight, 1));
        actualHeight = (int)(referenceHeight * scale);
		
		renderWidth = (int)(Screen.width / scale);

		actualWidth = (int)(renderWidth * scale);

		Rect rect = cam.rect;
		rect.width = (float)actualWidth / Screen.width;
		rect.height = (float)actualHeight / Screen.height;
		rect.x = (1 - rect.width) / 2;
		rect.y = (1 - rect.height) / 2;
		cam.rect = rect;
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (renderWidth > 0 && referenceHeight > 0)
        {
            RenderTexture buffer = RenderTexture.GetTemporary(renderWidth, referenceHeight, -1);

            buffer.filterMode = FilterMode.Point;
            source.filterMode = FilterMode.Point;

            Graphics.Blit(source, buffer);
            Graphics.Blit(buffer, destination);

            RenderTexture.ReleaseTemporary(buffer);
        }
	}
}