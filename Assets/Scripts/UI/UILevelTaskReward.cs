using TMPro;
using UnityEngine;

[RequireComponent(typeof(TransitionAnimator))]
public class UILevelTaskReward : MonoBehaviour
{
    public const string ShowCompletedTransitionName = "ShowCompleted";
    public const string ShowUncompletedTransitionName = "ShowUncompleted";
    
    public TransitionAnimator Animator { get; private set; }

    public TextMeshProUGUI taskText;
    public TextMeshProUGUI rewardText;

    private void Awake()
    {
        Animator = GetComponent<TransitionAnimator>();
    }
}
