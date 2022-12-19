using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TransitionAnimator))]
public class UIDialog : MonoBehaviour
{
    public enum DefaultExitButton
    {
        None, Positive, Negative, Neutral
    }

    private const string ShowTransitionName = "Show";
    private const string HideTransitionName = "Hide";

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button positiveButton;
    [SerializeField] private TextMeshProUGUI positiveButtonText;
    [SerializeField] private Button negativeButton;
    [SerializeField] private TextMeshProUGUI negativeButtonText;
    [SerializeField] private Button neutralButton;
    [SerializeField] private TextMeshProUGUI neutralButtonText;

    public static UIDialog Default => UIManager.Instance.DefaultUIDialog;

    private bool isShowed = false;
    private TransitionAnimator animator;
    private Action onPositiveButtonClick;
    private Action onNegativeButtonClick;
    private Action onNeutralButtonClick;
    private DefaultExitButton defaultExitButton;

    private void Awake()
    {
        animator = GetComponent<TransitionAnimator>();
    }

    private void Start()
    {
        animator.Play(HideTransitionName);
        animator.EndAnimationImmediately();

        positiveButton.onClick.AddListener(() =>
        {
            onPositiveButtonClick?.Invoke();
            HideLazy();
        });
        negativeButton.onClick.AddListener(() =>
        {
            onNegativeButtonClick?.Invoke();
            HideLazy();
        });
        neutralButton.onClick.AddListener(() =>
        {
            onNeutralButtonClick?.Invoke();
            HideLazy();
        });
    }

    private void OnDisable()
    {
        Hide();
        animator.EndAnimationImmediately();
    }

    public void Open(
        string title,
        string description,
        DefaultExitButton defaultExitButton,
        string positiveButton,
        Action onPositiveButtonClick=null,
        string negativeButton="",
        Action onNegativeButtonClick=null,
        string neutralButton="",
        Action onNeutralButtonClick=null)
    {
        isShowed = true;

        titleText.text = title;
        descriptionText.text = description;
        this.defaultExitButton = defaultExitButton;
        positiveButtonText.text = positiveButton;
        this.positiveButton.gameObject.SetActive(positiveButton.Length > 0);
        this.onPositiveButtonClick = onPositiveButtonClick;
        negativeButtonText.text = negativeButton;
        this.negativeButton.gameObject.SetActive(negativeButton.Length > 0);
        this.onNegativeButtonClick = onNegativeButtonClick;
        neutralButtonText.text = neutralButton;
        this.neutralButton.gameObject.SetActive(neutralButton.Length > 0);
        this.onNeutralButtonClick = onNeutralButtonClick;

        animator.Play(ShowTransitionName);
    }

    public void Hide()
    {
        if (!isShowed)
        {
            return;
        }

        switch (defaultExitButton)
        {
            case DefaultExitButton.Positive:
                onPositiveButtonClick?.Invoke();
                break;
            case DefaultExitButton.Negative:
                onNegativeButtonClick?.Invoke();
                break;
            case DefaultExitButton.Neutral:
                onNeutralButtonClick?.Invoke();
                break;
        }

        onPositiveButtonClick = null;
        onNegativeButtonClick = null;
        onNeutralButtonClick = null;

        HideLazy();
    }

    private void HideLazy()
    {
        isShowed = false;
        animator.Play(HideTransitionName);
    }
}