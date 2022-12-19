using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class CameraRenderImage : MonoBehaviour
{
    private RawImage image;

    private void Start()
    {
        image = GetComponent<RawImage>();

        CameraController.Instance.OnRenderScaleChange += (tex) => image.texture = tex;
    }
}
