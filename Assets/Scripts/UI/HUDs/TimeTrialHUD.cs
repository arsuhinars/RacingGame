using System.Text;
using TMPro;
using UnityEngine;

public class TimeTrialHUD : MonoBehaviour
{
    private const string BlinkingAnimation = "Blinking";
    private const string IdleAnimation = "Idle";
    private const float BlinkingTime = 10f;
    private const float FastBlinkingTime = 6f;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TransitionAnimator timerTextAnimator;

    private void Start()
    {
        GameManager.Instance.OnPause += () => timerTextAnimator.Stop();
        GameManager.Instance.OnGameEnd += (reason) => timerTextAnimator.Stop();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted)
            return;

        float timeRemain = GameManager.Instance.GetCounterValue(TimeTrialModeManager.RemainedTimeCounterName);
        if (timeRemain <= BlinkingTime && timerTextAnimator.CurrentAnimationName != BlinkingAnimation)
        {
            timerTextAnimator.Play(BlinkingAnimation, true);
        }
        else if (timeRemain > BlinkingTime && timerTextAnimator.CurrentAnimationName != IdleAnimation)
        {
            timerTextAnimator.Play(IdleAnimation, true);
        }
        timerTextAnimator.timeScale = timeRemain < FastBlinkingTime ? 1.5f : 1f;

        var builder = new StringBuilder(5);
        builder.AppendFormat("{0}", Mathf.FloorToInt(timeRemain / 60f));
        builder.Append(':');
        builder.AppendFormat("{0:00}", Mathf.FloorToInt(timeRemain) % 60);

        timerText.text = builder.ToString();
    }
}
