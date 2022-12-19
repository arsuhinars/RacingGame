using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TransitionAnimator))]
public class UIMessage : MonoBehaviour
{
    private const string ShowTransitionName = "Show";
    private const string HideTransitionName = "Hide";

    public static readonly Color BlackBackground = new(0f, 0f, 0f);
    public static readonly Color RedBackground = new(1.000f, 0.106f, 0.059f);
    public static readonly Color GreenBackground = new(0.408f, 1.000f, 0.071f);
    public static readonly Color BlueBackground = new(0.071f, 0.376f, 1.000f);
    public static readonly Color LightBlueBackground = new(0.137f, 0.490f, 0.941f);
    public static readonly Color PurpleBackground = new(0.824f, 0.071f, 1.000f);
    public static readonly Color YellowBackground = new(1.000f, 0.831f, 0.071f);
    
    public const float LongDuration = 3.4f;
    public const float ShortDuration = 1.9f;

    public static UIMessage Default => UIManager.Instance.DefaultUIMessage;

    [SerializeField] private TextMeshProUGUI textElement;
    [SerializeField] private Image backgroundImage;

    private TransitionAnimator animator;
    private Coroutine currentTimer;

    private void Awake()
    {
        animator = GetComponent<TransitionAnimator>();
    }

    private void Start()
    {
        animator.Play(HideTransitionName);
        animator.EndAnimationImmediately();
    }

    public void Show(string text, Color backgroundColor, float duration)
    {
        if (currentTimer != null)
            StopCoroutine(currentTimer);

        textElement.text = text;
        backgroundImage.color = backgroundColor;
        animator.Play(ShowTransitionName);
        currentTimer = StartCoroutine(HideAfterTime(duration));
    }

    public void Hide()
    {
        animator.Play(HideTransitionName);
    }

    private IEnumerator HideAfterTime(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        Hide();
    }
}
