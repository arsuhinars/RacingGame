using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    // Ссылка на представителя камеры
    public static CameraController Instance { get; private set; }

    public event Action<RenderTexture> OnRenderScaleChange;

    public float RenderScale
    {
        get => _renderScale;
        set
        {
            _renderScale = value;

            if (texture != null)
                texture.Release();

            texture = new RenderTexture(
                Mathf.FloorToInt(Screen.width * _renderScale),
                Mathf.FloorToInt(Screen.height * _renderScale), 16, RenderTextureFormat.Default
            );
            cam.targetTexture = texture;

            OnRenderScaleChange?.Invoke(texture);
        }
    }

    [SerializeField] private bool useSettingsForRenderScale = true;
    [SerializeField, Range(0.2f, 1f)] private float _renderScale = 1.0f;
    [Space]
    [SerializeField] private int highRenderHeight = 1080;
    [SerializeField] private int mediumRenderHeight = 720;
    [SerializeField] private int lowRenderHeight = 420;

    private Camera cam;
    private RenderTexture texture;

    private void Awake()
    {
        if (Instance != null)
            return;
        else Instance = this;
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        RenderScale = _renderScale;
        SettingsManager.Instance.OnSettingsRefresh += UpdateRenderScaleFromSettings;
    }

    private void UpdateRenderScaleFromSettings()
    {
        if (!useSettingsForRenderScale)
            return;

        float renderScale = SettingsManager.Instance.RenderQuality switch
        {
            SettingsManager.RenderQualityType.High => highRenderHeight,
            SettingsManager.RenderQualityType.Medium => mediumRenderHeight,
            _ => lowRenderHeight
        };
        renderScale /= Screen.height;

        RenderScale = Mathf.Min(1f, renderScale);
    }
}
