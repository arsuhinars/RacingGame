using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    private const string IsSoundEnabledPrefsKey = "IsSoundEnabled";
    private const string IsMusicEnabledPrefsKey = "IsMusicEnabled";
    private const string IsVibrationEnabledPrefsKey = "IsVibrationEnabled";
    private const string RenderQualityPrefsKey = "RenderQuality";
    private const string MaxFrameratePrefsKey = "MaxFramerate";
    
    public enum RenderQualityType
    {
        High, Medium, Low
    }
    private const RenderQualityType DefaultRenderQuality = RenderQualityType.Medium;

    public enum MaxFramerateType
    {
        FPS_30, FPS_45, FPS_60
    }
    private const MaxFramerateType DefaultMaxFramerate = MaxFramerateType.FPS_60;

    public event Action OnSettingsRefresh;

    public static SettingsManager Instance { get; private set; }

    public bool IsSoundEnabled
    {
        get => PlayerPrefs.GetInt(IsSoundEnabledPrefsKey, 1) != 0;
        set 
        {
            PlayerPrefs.SetInt(IsSoundEnabledPrefsKey, value ? 1 : 0);
            RefreshSettings();
        }
    }
    public bool IsMusicEnabled
    {
        get => PlayerPrefs.GetInt(IsMusicEnabledPrefsKey, 1) != 0;
        set
        {
            PlayerPrefs.SetInt(IsMusicEnabledPrefsKey, value ? 1 : 0);
            RefreshSettings();
        }
    }
    public bool IsVibrationEnabled
    {
        get => PlayerPrefs.GetInt(IsVibrationEnabledPrefsKey, 1) != 0;
        set
        {
            PlayerPrefs.SetInt(IsVibrationEnabledPrefsKey, value ? 1 : 0);
            RefreshSettings();
        }
    }
    public RenderQualityType RenderQuality
    {
        get => Utils.GetEnumFromPlayerPrefs(RenderQualityPrefsKey, DefaultRenderQuality);
        set
        {
            Utils.SetEnumInPlayerPrefs(RenderQualityPrefsKey, value);
            RefreshSettings();
        }
    }
    public MaxFramerateType MaxFramerate
    {
        get => Utils.GetEnumFromPlayerPrefs(MaxFrameratePrefsKey, DefaultMaxFramerate);
        set
        {
            Utils.SetEnumInPlayerPrefs(MaxFrameratePrefsKey, value);
            RefreshSettings();
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;
    }

    private void Start()
    {
        RefreshSettings();
    }

    public void RefreshSettings()
    {
        Application.targetFrameRate = MaxFramerate switch
        {
            MaxFramerateType.FPS_30 => 30,
            MaxFramerateType.FPS_45 => 45,
            MaxFramerateType.FPS_60 => 60,
            _ => 60
        };

        OnSettingsRefresh?.Invoke();
    }
}
