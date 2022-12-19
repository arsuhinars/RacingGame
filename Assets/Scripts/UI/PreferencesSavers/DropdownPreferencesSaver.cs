using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownPreferencesSaver : MonoBehaviour
{
    [SerializeField] private string preferencesKey;
    [SerializeField] private int defaultIndex;
    [SerializeField] private List<string> values;
    [SerializeField] private UnityEvent onValueSaved;

    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        int startIndex = values.IndexOf(PlayerPrefs.GetString(preferencesKey));
        if (startIndex == -1)
            startIndex = defaultIndex;

        dropdown.value = startIndex;
        dropdown.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(int index)
    {
        PlayerPrefs.SetString(preferencesKey, values[index]);
        onValueSaved?.Invoke();
    }
}