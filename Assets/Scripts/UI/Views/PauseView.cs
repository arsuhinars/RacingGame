using UnityEngine;
using UnityEngine.Localization.Settings;

public class PauseView : MonoBehaviour
{
    private const string CounterValueVariable = "value";

    [SerializeField] private UIGameCounter counterPrefab;
    [SerializeField] private Transform countersRoot;

    private void Start()
    {
        GameManager.Instance.OnLoaded += () =>
        {
            var defaultTable = LocalizationSettings.StringDatabase.DefaultTable;

            foreach (var counterName in GameManager.Instance.GameCountersNames)
            {
                var counter = GameManager.Instance.GetCounterData(counterName);
                if (counter.isHiddenInPause)
                    continue;

                var counterElement = Instantiate(counterPrefab, countersRoot);
                counterElement.nameText.text = LocalizationHelper.GetLocalizedString(counterName);

                var localizedStr = LocalizationHelper.MakeStringClone(counter.valueText);
                localizedStr[CounterValueVariable] = LocalizationHelper.GlobalVariables[counterName];

                counterElement.valueTextLocalizer.StringReference = localizedStr;
            }
        };
    }
}
