using UnityEngine;

public class SettingsView : MonoBehaviour
{
    private const string AboutAuthorDialogTitleLocale = "aboutAuthorDialogTitle";
    private const string AboutAuthorDialogDescriptionLocale = "aboutAuthorDialogDescription";
    private const string CloseLocale = "close";

    public void ShowAuthorDialog()
    {
        UIDialog.Default.Open(
            LocalizationHelper.GetLocalizedString(AboutAuthorDialogTitleLocale),
            LocalizationHelper.GetLocalizedString(AboutAuthorDialogDescriptionLocale),
            UIDialog.DefaultExitButton.Positive,
            LocalizationHelper.GetLocalizedString(CloseLocale)
        );
    }
}
