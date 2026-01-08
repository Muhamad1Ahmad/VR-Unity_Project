using UnityEngine;

public class ProximityGuidancePrompt : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private StepDialogueUI dialogueUI;

    [Header("Message")]
    [TextArea(2, 5)]
    [SerializeField] private string message = "Do something here...";
    [Min(0.1f)]
    [SerializeField] private float showForSeconds = 2.5f;

    [Header("Proximity")]
    [Tooltip("If empty, will use XR main camera automatically.")]
    [SerializeField] private Transform playerHead;

    [Tooltip("Distance in meters to trigger message.")]
    [Min(0.1f)]
    [SerializeField] private float triggerDistance = 1.5f;

    [Tooltip("Show only once (recommended).")]
    [SerializeField] private bool showOnlyOnce = true;

    [Tooltip("Cooldown to avoid spamming if player stays near.")]
    [Min(0f)]
    [SerializeField] private float cooldownSeconds = 2f;

    private bool _hasShown;
    private float _cooldownTimer;

    private void Awake()
    {
        if (playerHead == null && Camera.main != null)
            playerHead = Camera.main.transform;
    }

    private void Update()
    {
        if (dialogueUI == null || playerHead == null) return;

        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.unscaledDeltaTime;
            return;
        }

        if (showOnlyOnce && _hasShown) return;

        float d = Vector3.Distance(playerHead.position, transform.position);
        if (d <= triggerDistance)
        {
            dialogueUI.ShowCustom(message, showForSeconds);
            _hasShown = true;
            _cooldownTimer = cooldownSeconds;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
#endif
}
