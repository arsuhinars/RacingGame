using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownLocalizer : MonoBehaviour
{
    [Serializable]
    public class OptionsData
    {
        public LocalizedString text;
        public Sprite image;
    }

    [SerializeField]
    private List<OptionsData> _options;

    public List<OptionsData> Options
    {
        get => _options;
        set
        {
            _options = value;
            RefreshDropdown();
        }
    }

    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        RefreshDropdown();
    }

    public void RefreshDropdown()
    {
        var optionsData = new List<TMP_Dropdown.OptionData>(_options.Count);

        foreach (var option in _options)
        {
            optionsData.Add(new TMP_Dropdown.OptionData(
                option.text.GetLocalizedString(), option.image
            ));
        }

        dropdown.options = optionsData;
    }
}
