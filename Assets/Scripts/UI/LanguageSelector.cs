using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(TMP_Dropdown))]
public class LanguageSelector : MonoBehaviour
{
    private const string SelectedLocalePrefsKey = "SelectedLocale";
    private const string MessageLocale = "languageSettingMessage";

    private TMP_Dropdown dropdown;

    private void Awake()
    {
        dropdown = GetComponent<TMP_Dropdown>();
    }

    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        var options = new List<TMP_Dropdown.OptionData>(locales.Count);

        foreach (var locale in locales)
        {
            options.Add(new TMP_Dropdown.OptionData(locale.LocaleName));
        }

        dropdown.options = options;

        var selectedLocaleIndex = locales.IndexOf(LocalizationSettings.SelectedLocale);
        dropdown.value = selectedLocaleIndex;
        dropdown.onValueChanged.AddListener((localeIndex) =>
        {
            var messageText = LocalizationSettings.StringDatabase.GetLocalizedString(
                MessageLocale,
                locales[localeIndex]
            );

            UIMessage.Default.Show(messageText, UIMessage.BlackBackground, UIMessage.ShortDuration);
            PlayerPrefs.SetString(SelectedLocalePrefsKey, locales[localeIndex].Identifier.Code);
        });
    }
}
