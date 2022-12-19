using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

[RequireComponent(typeof(UIView))]
public class GameEndView : MonoBehaviour
{
    private const string AppearAnimation = "OnAppear";
    private const string HiddenAnimation = "Hidden";


    [SerializeField] private TextMeshProUGUI recordCounterText;
    [SerializeField] private LocalizeStringEvent recordCounterValue;
    [SerializeField] private LocalizeStringEvent recordValue;
    [SerializeField] private GameObject tasksContainer;
    [SerializeField] private GameObject rewardsContainer;
    [SerializeField] private UILevelTaskReward uiTaskRewardPrefab;
    [SerializeField] private TransitionAnimator moneyReward;
    [SerializeField] private TransitionAnimator reputationReward;
    [SerializeField] private TransitionAnimator sparesReward;

    private UIView view;
    private UILevelTaskReward[] uiTaskRewards;

    private void Awake()
    {
        view = GetComponent<UIView>();
        view.OnShow.AddListener(() =>
        {
            var recordVar = recordValue.StringReference[GameModeCounter.ValueVariableName] as FloatVariable;
            recordVar.Value = GameManager.Instance.LevelData.SaveData.recordValue;

            if (GameManager.Instance.LevelTasks.Length == 0)
                StartCoroutine(ShowRewards());
            else
                StartCoroutine(ShowTasks());
        });
    }

    private void Start()
    {
        GameManager.Instance.OnLoaded += () =>
        {
            var recordCounter = GameManager.Instance.GetCounterData(
                GameManager.Instance.GameModeConfig.recordCounterName
            );
            var recordValueString = LocalizationHelper.MakeStringClone(recordCounter.valueText);
            recordValueString[GameModeCounter.ValueVariableName] = LocalizationHelper.GlobalVariables[recordCounter.name];

            recordCounterText.text = LocalizationHelper.GetLocalizedString(recordCounter.name);
            recordCounterValue.StringReference = recordValueString;

            recordValueString = LocalizationHelper.MakeStringClone(recordCounter.valueText);
            recordValueString[GameModeCounter.ValueVariableName] = new FloatVariable();
            recordValue.StringReference = recordValueString;

            var levelTasks = GameManager.Instance.LevelTasks;

            uiTaskRewards = new UILevelTaskReward[levelTasks.Length];
            for (int i = 0; i < levelTasks.Length; i++)
            {
                var uiTaskReward = Instantiate(uiTaskRewardPrefab, tasksContainer.transform);
                uiTaskReward.taskText.text = levelTasks[i].GetLocalizedTaskText();
                uiTaskReward.rewardText.text = levelTasks[i].GetLocalizedRewardText();

                uiTaskRewards[i] = uiTaskReward;
            }
        };
    }

    private IEnumerator ShowTasks()
    {
        tasksContainer.SetActive(true);
        rewardsContainer.SetActive(false);

        // Заранее скрываем все задания
        for (int i = 0; i < uiTaskRewards.Length; i++)
        {
            uiTaskRewards[i].Animator.Play(UILevelTaskReward.ShowUncompletedTransitionName);
        }
        
        yield return new WaitForSecondsRealtime(0.8f);

        for (int i = 0; i < uiTaskRewards.Length; i++)
        {
            if (GameManager.Instance.LevelData.IsTaskCompleted(i))
            {
                uiTaskRewards[i].Animator.Play(UILevelTaskReward.ShowCompletedTransitionName);
                yield return new WaitForSecondsRealtime(0.65f);
            }
        }
    }

    private IEnumerator ShowRewards()
    {
        rewardsContainer.SetActive(true);
        tasksContainer.SetActive(false);

        moneyReward.Play(HiddenAnimation);
        reputationReward.Play(HiddenAnimation);
        sparesReward.Play(HiddenAnimation);

        reputationReward.gameObject.SetActive(GameManager.Instance.ReputationReward > 0);
        sparesReward.gameObject.SetActive(GameManager.Instance.SparesReward > 0);

        yield return new WaitForSecondsRealtime(0.8f);

        moneyReward.Play(AppearAnimation);

        if (GameManager.Instance.ReputationReward > 0)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            reputationReward.Play(AppearAnimation);
        }

        if (GameManager.Instance.SparesReward > 0)
        {
            yield return new WaitForSecondsRealtime(0.6f);
            sparesReward.Play(AppearAnimation);
        }
    }
}