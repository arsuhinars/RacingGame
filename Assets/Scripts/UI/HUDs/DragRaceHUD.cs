using System.Text;
using TMPro;
using UnityEngine;

public class DragRaceHUD : MonoBehaviour
{
    private const string MeterUnitLocale = "meterShort";

    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform carIcon;

    private string distanceUnitString;
    private Vector2 baseIconOffset;
    private Vector2 iconSize;
    private float progressBarSizeX;

    private void Start()
    {
        distanceUnitString = LocalizationHelper.GetLocalizedString(MeterUnitLocale);

        baseIconOffset = carIcon.offsetMin;
        iconSize = carIcon.sizeDelta;
        progressBarSizeX = progressBar.sizeDelta.x;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted)
            return;

        float remainedDist = GameManager.Instance.GetCounterValue(
            DragRaceModeManager.RemainedDistanceCounterName
        );

        var builder = new StringBuilder(32);
        builder.Append(Mathf.FloorToInt(remainedDist));
        builder.Append(' ');
        builder.Append(distanceUnitString);

        distanceText.text = builder.ToString();

        // Перемещаем иконку машины на прогресс баре
        float iconOffset = progressBarSizeX - baseIconOffset.x * 2f - iconSize.x;
        iconOffset *= 1f - remainedDist / DragRaceModeManager.Instance.TargetDistance;

        carIcon.offsetMin = baseIconOffset + Vector2.right * iconOffset;
        carIcon.offsetMax = carIcon.offsetMin + iconSize;
    }
}
