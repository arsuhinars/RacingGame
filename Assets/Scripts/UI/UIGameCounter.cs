using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class UIGameCounter : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public LocalizeStringEvent valueTextLocalizer;
    [HideInInspector] public FloatVariable valueVariable;
}
