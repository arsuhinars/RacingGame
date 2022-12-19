using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform backgroundLine;
    [SerializeField] private RectTransform progressLine;

    public float Value
    {
        get => _value;
        set
        {
            _value = value;
            UpdateProgressLine(value);
        }
    }
    private float _value;

    private void Start()
    {
        UpdateProgressLine(Value);
    }

    private void UpdateProgressLine(float value)
    {
        float lineWidth = backgroundLine.sizeDelta.x * Mathf.Clamp01(Value);

        progressLine.offsetMin = backgroundLine.offsetMin + Vector2.right * lineWidth;
        progressLine.offsetMax = backgroundLine.offsetMax;
    }
}
