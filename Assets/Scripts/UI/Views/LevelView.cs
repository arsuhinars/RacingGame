using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UI;

public class LevelMenuViewData
{
    public UILevelCard levelCardInstance;
}

[RequireComponent(typeof(UIView))]
public class LevelView : MonoBehaviour
{
    private const string MoreButtonTextKey = "moreButton";
    private const string TasksButtonTextKey = "tasksButton";
    private const string TrafficDensityValueKey = "trafficDensityValue";
    private const string TrafficSpeedValueKey = "trafficSpeedValue";

    [SerializeField] private RectTransform levelCardRect;
    [SerializeField] private UIView tasksView;
    [SerializeField] private UIView moreView;
    [SerializeField] private Transform tasksList;
    [SerializeField] private UILevelTaskReward taskRewardPrefab;
    [SerializeField] private Button moreButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI recordValueText;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private TextMeshProUGUI trafficDensityText;
    [SerializeField] private TextMeshProUGUI trafficSpeedText;

    private UIView view;
    private UILevelCard levelCard;
    private readonly List<UILevelTaskReward> uiTaskRewards = new();
    private bool isMoreViewOpened = false;
    private TextMeshProUGUI moreButtonTextElement;

    private string moreButtonText;
    private string tasksButtonText;

    private void Awake()
    {
        view = GetComponent<UIView>();
    }

    private void Start()
    {
        moreButtonText = LocalizationHelper.GetLocalizedString(MoreButtonTextKey);
        tasksButtonText = LocalizationHelper.GetLocalizedString(TasksButtonTextKey);

        moreButtonTextElement = moreButton.GetComponentInChildren<TextMeshProUGUI>();
        moreButton.onClick.AddListener(() =>
        {
            isMoreViewOpened = !isMoreViewOpened;
            if (isMoreViewOpened)
            {
                moreButtonTextElement.text = tasksButtonText;
                moreView.Show();
                tasksView.Hide();
            }
            else
            {
                moreButtonTextElement.text = moreButtonText;
                moreView.Hide();
                tasksView.Show();
            }
        });
        startButton.onClick.AddListener(() =>
        {
            if (levelCard != null)
                MainMenuManager.Instance.StartLevel(levelCard.LevelData);
        });

        view.OnShow.AddListener(() =>
        {
            var viewData = view.viewData as LevelMenuViewData;
            if (viewData == null)
                return;

            levelCard = Instantiate(viewData.levelCardInstance, transform);
            
            // Создаем карточку уровня
            var levelCardRect = levelCard.GetComponent<RectTransform>();
            levelCardRect.anchorMin = this.levelCardRect.anchorMin;
            levelCardRect.anchorMax = this.levelCardRect.anchorMax;
            levelCardRect.offsetMin = this.levelCardRect.offsetMin;
            levelCardRect.offsetMax = this.levelCardRect.offsetMax;
            levelCard.enabled = false;

            var levelData = levelCard.LevelData;
            var gmConfigHandle = levelData.GetGameModeConfigHandle();
            gmConfigHandle.WaitForCompletion();

            var gmConfig = gmConfigHandle.Result;

            // Создаем список задач уровня
            var levelTasks = levelData.tasks;
            for (int i = uiTaskRewards.Count; i < levelTasks.Length; i++)
            {
                uiTaskRewards.Add(Instantiate(taskRewardPrefab, tasksList));
            }

            for (int i = levelTasks.Length; i < uiTaskRewards.Count; i++)
            {
                uiTaskRewards[i].gameObject.SetActive(false);
            }

            if (levelTasks.Length > 0)
            {
                for (int i = 0; i < levelTasks.Length; i++)
                {
                    uiTaskRewards[i].gameObject.SetActive(true);
                    uiTaskRewards[i].taskText.text = levelTasks[i].GetLocalizedTaskText();
                    uiTaskRewards[i].rewardText.text = levelTasks[i].GetLocalizedRewardText();
                    uiTaskRewards[i].Animator.Play(
                        levelData.SaveData.maxCompletedTaskIndex >= i ?
                        UILevelTaskReward.ShowCompletedTransitionName :
                        UILevelTaskReward.ShowUncompletedTransitionName
                    );
                    uiTaskRewards[i].Animator.EndAnimationImmediately();
                }

                var recordCounter = gmConfig.GetCounterByName(gmConfig.recordCounterName);
                var recordString = LocalizationHelper.MakeStringClone(recordCounter.valueText);
                recordString[GameModeCounter.ValueVariableName] = new FloatVariable()
                {
                    Value = levelData.SaveData.recordValue
                };

                recordValueText.text = recordString.GetLocalizedString();

                moreButton.gameObject.SetActive(true);
                moreButtonTextElement.text = moreButtonText;
                moreView.Hide(false);
                tasksView.Show(false);
                isMoreViewOpened = false;
            }
            else
            {
                moreButton.gameObject.SetActive(false);
                moreView.Show(false);
                tasksView.Hide(false);
            }

            Addressables.Release(gmConfigHandle);

            mapNameText.text = LocalizationHelper.GetLocalizedString(levelData.mapName);
            trafficDensityText.text = LocalizationHelper.GetLocalizedString(TrafficDensityValueKey, 
                new Dictionary<string, IVariable>()
                {
                    { GameModeCounter.ValueVariableName, new FloatVariable() { Value = levelData.trafficDensity * 100f} }
                }
            );
            trafficSpeedText.text = LocalizationHelper.GetLocalizedString(TrafficSpeedValueKey,
                new Dictionary<string, IVariable>()
                {
                    { GameModeCounter.ValueVariableName, new FloatVariable() { Value = levelData.trafficSpeed * 100f} }
                }
            );
        });
        view.OnHide.AddListener(() =>
        {
            if (levelCard != null)
            {
                Destroy(levelCard.gameObject);
                levelCard = null;
            }
        });
    }
}