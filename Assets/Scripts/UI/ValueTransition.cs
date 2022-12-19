using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ValueTransition
{
    public enum EasingType
    {
        Linear,
        SinIn, SinOut, SinInOut,
        QuadIn, QuadOut, QuadInOut,
        CubicIn, CubicOut, CubicInOut,
        QuartIn, QuartOut, QuartInOut
    }

    public enum ValueType
    {
        Float, Vector2, Vector3, Color, Quaternion, Bool
    }

    public UnityEvent<float> FloatSetter => _floatSetter;
    public UnityEvent<Vector2> Vector2Setter => _vector2Setter;
    public UnityEvent<Vector3> Vector3Setter => _vector3Setter;
    public UnityEvent<Color> ColorSetter => _colorSetter;
    public UnityEvent<Quaternion> QuaternionSetter => _quaternionSetter;
    public UnityEvent<bool> BoolSetter => _boolSetter;
    public bool DidEnd => time >= transitionTime;

    // Параметры в инспекторе
    [SerializeField] private UnityEvent<float> _floatSetter = new();
    [SerializeField] private UnityEvent<Vector2> _vector2Setter = new();
    [SerializeField] private UnityEvent<Vector3> _vector3Setter = new();
    [SerializeField] private UnityEvent<Color> _colorSetter = new();
    [SerializeField] private UnityEvent<Quaternion> _quaternionSetter = new();
    [SerializeField] private UnityEvent<bool> _boolSetter = new();
    public Vector4 startValue;
    public Vector4 endValue;
    public float transitionTime;
    public bool playWithPrevious = false;
    public EasingType easingFunction;
    public ValueType valueType;

    [HideInInspector] public float time;
    [HideInInspector] public bool isRealtime;
    [HideInInspector] public float timeScale;

    public void HandleTransition(float time)
    {
        if (startValue == null || endValue == null)
            return;

        float t = Mathf.Clamp01(GetEasing(time));

        switch (valueType)
        {
            case ValueType.Float:
                FloatSetter?.Invoke(Mathf.Lerp(startValue[0], endValue[0], t));
                break;
            case ValueType.Vector2:
                Vector2Setter?.Invoke(Vector2.Lerp(startValue, endValue, t));
                break;
            case ValueType.Vector3:
                Vector3Setter?.Invoke(Vector3.Lerp(startValue, endValue, t));
                break;
            case ValueType.Color:
                ColorSetter?.Invoke(Color.Lerp(startValue, endValue, t));
                break;
            case ValueType.Quaternion:
                QuaternionSetter?.Invoke(
                    Quaternion.Lerp(Utils.QuaternionFromVector4(startValue), Utils.QuaternionFromVector4(endValue), t)
                );
                break;
            case ValueType.Bool:
                BoolSetter?.Invoke((t < 1.0f && startValue[0] >= 1.0f) || (t >= 1.0f && endValue[0] >= 1.0f));
                break;
        }
    }

    public void Update()
    {
        if (DidEnd)
            return;

        time += (isRealtime ? Time.unscaledDeltaTime : Time.deltaTime) * timeScale;
        HandleTransition(time / transitionTime);
    }

    private float GetEasing(float t)
    {
        return easingFunction switch
        {
            EasingType.SinIn => Easings.EaseInSine(t),
            EasingType.SinOut => Easings.EaseOutSine(t),
            EasingType.SinInOut => Easings.EaseInOutSine(t),

            EasingType.QuadIn => Easings.EaseInQuad(t),
            EasingType.QuadOut => Easings.EaseOutQuad(t),
            EasingType.QuadInOut => Easings.EaseInOutQuad(t),

            EasingType.CubicIn => Easings.EaseInCubic(t),
            EasingType.CubicOut => Easings.EaseOutCubic(t),
            EasingType.CubicInOut => Easings.EaseInOutCubic(t),

            EasingType.QuartIn => Easings.EaseInQuart(t),
            EasingType.QuartOut => Easings.EaseOutQuart(t),
            EasingType.QuartInOut => Easings.EaseInOutQuart(t),

            _ => t
        };
    }
}
