using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    public int amount = 100;
    [SerializeField] private string _reason;
    public float cooldown = 0.1f;
    public float minSpeed = 0.0f;
    public bool onlyOnTrigger = true;
    public bool countOnlyOnce = true;

    public string Reason
    {
        get => _reason;
        set
        {
            _reason = value;
            localizedReason = LocalizationHelper.GetLocalizedString(value);
        }
    }

    private bool didCounted = false;
    private float cooldownTimer = 0.0f;

    private bool CanTrigger => (!countOnlyOnce || !didCounted) && cooldownTimer < 0.0f && MapManager.Instance.MoveSpeed >= minSpeed;
    private string localizedReason;

    private void Start()
    {
        Reason = _reason;
    }

    private void OnDisable()
    {
        didCounted = false;
    }

    private void Update()
    {
        if (!GameManager.Instance.IsGameStarted)
            return;

        cooldownTimer -= Time.deltaTime;

        if (!onlyOnTrigger && CanTrigger)
        {
            didCounted = true;
            cooldownTimer = cooldown;
            GameManager.Instance.AddScore(amount, localizedReason);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!GameManager.Instance.IsGameStarted)
            return;

        if (onlyOnTrigger && other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player") && CanTrigger)
        {
            didCounted = true;
            cooldownTimer = cooldown;
            GameManager.Instance.AddScore(amount, localizedReason);
        }
    }
}
