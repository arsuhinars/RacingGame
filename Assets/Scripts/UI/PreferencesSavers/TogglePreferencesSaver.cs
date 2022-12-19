using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class TogglePreferencesSaver : MonoBehaviour
{
    [SerializeField] private string preferencesKey;
    [SerializeField] private bool defaultValue;
    [SerializeField] private UnityEvent onValueSaved;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void Start()
    {
        toggle.isOn = PlayerPrefs.GetInt(preferencesKey, defaultValue ? 1 : 0) != 0;
        toggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(bool value)
    {
        PlayerPrefs.SetInt(preferencesKey, value ? 1 : 0);
        onValueSaved?.Invoke();
    }
}