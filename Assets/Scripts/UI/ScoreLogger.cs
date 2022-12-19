using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System.Text;

public class ScoreLogger : MonoBehaviour
{
    private const string ShowTransitionName = "Show";
    private const string HideTransitionName = "Hide";

    [SerializeField] private GameObject scoreLinePrefab;
    [SerializeField] private int maxLinesCount;
    [SerializeField] private float disappearTime;

    private class ScoreLine
    {
        public GameObject gameObject;
        public Transform transform;
        public TransitionAnimator animator;
        public TextMeshProUGUI textComponent;
        public float addTime;
    }

    private ObjectPool<ScoreLine> pool;
    private readonly LinkedList<ScoreLine> activeLines = new();
    private bool wasLastLineRemoved = true;

    private void Start()
    {
        pool = new ObjectPool<ScoreLine>(
            () =>
            {
                var scoreLineObj = Instantiate(scoreLinePrefab, transform);
                return new ScoreLine()
                {
                    gameObject = scoreLineObj,
                    transform = scoreLineObj.transform,
                    animator = scoreLineObj.GetComponent<TransitionAnimator>(),
                    textComponent = scoreLineObj.GetComponent<TextMeshProUGUI>()
                };
            },
            (scoreLine) =>
            {
                scoreLine.addTime = Time.time;
                scoreLine.animator.Play(ShowTransitionName);
                scoreLine.transform.SetAsFirstSibling();
                activeLines.AddFirst(scoreLine);
            },
            (scoreLine) => { },
            (scoreLine) => Destroy(scoreLine.gameObject)
        );

        GameManager.Instance.OnGameStart += () =>
        {
            foreach (var line in activeLines)
                pool.Release(line);
            activeLines.Clear();
        };

        GameManager.Instance.OnAddScore += (int amount, string name) =>
        {
            var builder = new StringBuilder(32);
            builder.Append('+');
            builder.Append(amount);
            builder.Append(' ');
            builder.Append(name);

            pool.Get().textComponent.text = builder.ToString();
        };
    }

    private void Update()
    {
        if (activeLines.Last == null || !wasLastLineRemoved)
            return;

        var lastLine = activeLines.Last;
        if (activeLines.Count > maxLinesCount || Time.time - lastLine.Value.addTime > disappearTime)
        {
            wasLastLineRemoved = false;
            lastLine.Value.animator.Play(HideTransitionName, () =>
            {
                wasLastLineRemoved = true;
                pool.Release(lastLine.Value);
                activeLines.Remove(lastLine);
            });
        }
    }
}
