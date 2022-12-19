using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class UILevelCard : MonoBehaviour, IPointerUpHandler
{
    public LevelData LevelData => levelData;

    [SerializeField] private LevelData levelData;
    [SerializeField] private GameObject recordContainer;
    [SerializeField] private GameObject starsContainer;
    [SerializeField] private GameObject grayStarPrefab;
    [SerializeField] private GameObject highlightedStarPrefab;
    [SerializeField] private Image gameModeIcon;
    [SerializeField] private TextMeshProUGUI gameModeName;
    [SerializeField] private TextMeshProUGUI recordValue;

    private void Start()
    {
        var gmConfigHandle = levelData.GetGameModeConfigHandle();
        gmConfigHandle.WaitForCompletion();

        var gmConfig = gmConfigHandle.Result;

        gameModeIcon.sprite = gmConfig.gameModeIcon;
        gameModeName.text = LocalizationHelper.GetLocalizedString(gmConfig.gameModeName);

        if (levelData.tasks.Length == 0)
        {
            recordContainer.SetActive(true);
            starsContainer.SetActive(false);

            var recordCounter = gmConfig.GetCounterByName(gmConfig.recordCounterName);
            if (recordCounter.valueText.ContainsKey(GameModeCounter.ValueVariableName))
                recordCounter.valueText.Remove(GameModeCounter.ValueVariableName);
            recordCounter.valueText[GameModeCounter.ValueVariableName] = new FloatVariable()
            {
                Value = levelData.SaveData.recordValue
            };
            recordValue.text = recordCounter.valueText.GetLocalizedString();
        }
        else
        {
            starsContainer.SetActive(true);
            recordContainer.SetActive(false);

            for (int i = 0; i <= levelData.SaveData.maxCompletedTaskIndex; i++)
                Instantiate(highlightedStarPrefab, starsContainer.transform);

            for (int i = levelData.SaveData.maxCompletedTaskIndex + 1; i < levelData.tasks.Length; i++)
                Instantiate(grayStarPrefab, starsContainer.transform);
        }

        Addressables.Release(gmConfigHandle);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.selectedObject == gameObject && !eventData.dragging)
        {
            UIManager.Instance.OpenView(MainMenuManager.LevelViewName, new LevelMenuViewData()
            {
                levelCardInstance = this
            });
        }
    }
}
