using UnityEngine;
using UnityEngine.Events;

public class UIView : MonoBehaviour
{
    private const string ShowTransitionName = "Show";
    private const string HideTransitionName = "Hide";

    public bool IsShowed { get; private set; } = true;
    public UnityEvent OnShow => _onShow;
    public UnityEvent OnHide => _onHide;

    [HideInInspector] public object viewData;

    [SerializeField] private bool startHidden = true;
    [SerializeField] private UnityEvent _onShow;
    [SerializeField] private UnityEvent _onHide;

    private TransitionAnimator animator;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        animator = GetComponent<TransitionAnimator>();
        canvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (startHidden)
            Hide(false);
        else
            Show(false);
    }
    
    public void Show(bool playTransition = true)
    {
        if (IsShowed)
            return;

        IsShowed = true;

        if (animator != null)
        {
            animator.Play(ShowTransitionName);
            if (!playTransition)
                animator.EndAnimationImmediately();
        }

        if (canvas != null)
            canvas.enabled = true;

        if (canvasGroup != null)
        {
            // canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        OnShow.Invoke();
    }

    public void Hide(bool playTransition = true)
    {
        if (!IsShowed)
            return;

        IsShowed = false;

        if (canvasGroup != null)
        {
            // canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (animator != null)
        {
            animator.Play(HideTransitionName, () =>
            {
                if (canvas != null)
                {
                    canvas.enabled = false;
                }
            });
            if (!playTransition)
                animator.EndAnimationImmediately();
        }
        else if (canvas != null)
            canvas.enabled = false;

        OnHide.Invoke();
    }

    public void Toggle(bool playTransition = true)
    {
        if (IsShowed)
            Hide(playTransition);
        else
            Show(playTransition);
    }

#if UNITY_EDITOR
    [ContextMenu("Show")]
    private void DebugShow()
    {
        Show();
    }

    [ContextMenu("Hide")]
    private void DebugHide()
    {
        Hide();
    }
#endif
}
