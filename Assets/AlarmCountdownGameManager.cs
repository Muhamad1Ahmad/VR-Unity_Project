using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class AlarmCountdownGameManager : MonoBehaviour
{

    [Header("Failure Center Message")]
    [Tooltip("Big center text shown when timer ends (e.g., 'You failed to control the fire').")]
    [SerializeField] private TMP_Text centerFailureText;

    [Tooltip("Message shown in the center when time ends.")]
    [TextArea]
    [SerializeField] private string centerFailureMessage = "YOU FAILED TO CONTROL THE FIRE.";

    [Tooltip("How long to show the center message (seconds). 0 = stay visible.")]
    [SerializeField] private float centerMessageDuration = 0f;

    [Tooltip("Optional: also hide the timer when failure happens.")]
    [SerializeField] private bool hideTimerOnFailure = true;


    [Header("References")]
    [Tooltip("Text that displays the countdown (TMP Text).")]
    [SerializeField] private TMP_Text timerText;

    [Tooltip("Optional label above timer like 'Evacuation Timer' (TMP Text).")]
    [SerializeField] private TMP_Text timerLabelText;

    [Tooltip("Window to show when time runs out (Failed_Window).")]
    [SerializeField] private GameObject failedWindow;

    [Tooltip("Text inside the failed window for the reason.")]
    [SerializeField] private TMP_Text failureReasonText;

    [Header("Timer Settings")]
    [Tooltip("Total time (seconds) after alarm trigger before failing.")]
    [Min(1f)]
    [SerializeField] private float totalSeconds = 60f;

    [Tooltip("Update the visible timer each N seconds (1 = once per second).")]
    [Min(0.05f)]
    [SerializeField] private float uiTickInterval = 1f;

    [Tooltip("Turn timer red when remaining time is <= this value (seconds).")]
    [Min(0f)]
    [SerializeField] private float redInLastSeconds = 10f;

    [Header("UI Style")]
    [Tooltip("Color when timer is normal.")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Color when timer is in the last X seconds.")]
    [SerializeField] private Color dangerColor = new Color(1f, 0.2f, 0.2f, 1f);

    [Tooltip("Optional: Pulse animation in danger time.")]
    [SerializeField] private bool pulseInDanger = true;

    [Tooltip("Pulse speed (higher = faster).")]
    [Min(0.1f)]
    [SerializeField] private float pulseSpeed = 3f;

    [Tooltip("Min scale during pulse (1 = no scale change).")]
    [Range(0.7f, 1f)]
    [SerializeField] private float pulseMinScale = 0.92f;

    [Header("Behavior")]
    [Tooltip("Hide timer UI until alarm is triggered.")]
    [SerializeField] private bool hideTimerUntilStart = true;

    [Tooltip("Automatically open the failed window when time ends.")]
    [SerializeField] private bool showFailureWindowOnEnd = true;

    [Tooltip("Failure message to show when timer finishes.")]
    [TextArea]
    [SerializeField]
    private string timeoutFailureMessage =
        "TIME OUT!\nYou did not trigger the correct safety steps in time.";

    // State
    private Coroutine _timerRoutine;
    private float _remaining;
    private bool _running;

    private Vector3 _timerTextBaseScale;
    private Vector3 _labelTextBaseScale;

    private void Awake()
    {

        if (centerFailureText != null)
            centerFailureText.gameObject.SetActive(false);

        if (timerText != null) _timerTextBaseScale = timerText.transform.localScale;
        if (timerLabelText != null) _labelTextBaseScale = timerLabelText.transform.localScale;

        if (failedWindow != null) failedWindow.SetActive(false);

        if (hideTimerUntilStart)
            SetTimerUIVisible(false);
        else
            SetTimerUIVisible(true);

        UpdateTimerUI(totalSeconds);
    }

    // Call this from your Fire Alarm trigger
    public void StartAlarmCountdown()
    {
        if (_running) return;

        _running = true;
        _remaining = totalSeconds;

        if (hideTimerUntilStart) SetTimerUIVisible(true);

        // reset UI look
        ApplyNormalStyle();
        UpdateTimerUI(_remaining);

        _timerRoutine = StartCoroutine(TimerRoutine());
    }

    // If the user succeeds before time runs out
    public void CancelAlarmCountdown()
    {
        if (!_running) return;

        _running = false;

        if (_timerRoutine != null)
            StopCoroutine(_timerRoutine);

        _timerRoutine = null;

        // Optional: hide timer again after success
        if (hideTimerUntilStart) SetTimerUIVisible(false);

        // Reset styling
        ApplyNormalStyle();
    }

    // Optional: you can also manually force failure from other scripts
    public void TriggerFailure(string reason)
    {
        // Show failed window
        if (failedWindow != null)
            failedWindow.SetActive(true);

        // Set failed reason inside window
        if (failureReasonText != null)
            failureReasonText.text = string.IsNullOrWhiteSpace(reason) ? timeoutFailureMessage : reason;

        // Optional: hide timer UI
        if (hideTimerOnFailure)
            SetTimerUIVisible(false);

        // Show center message
        if (centerFailureText != null)
        {
            centerFailureText.text = centerFailureMessage;
            centerFailureText.gameObject.SetActive(true);

            if (centerMessageDuration > 0f)
                StartCoroutine(HideCenterMessageAfterDelay(centerMessageDuration));
        }
    }

    private IEnumerator HideCenterMessageAfterDelay(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);

        // Hide center text
        if (centerFailureText != null)
            centerFailureText.gameObject.SetActive(false);

        // Reload current scene (Try Again)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    private IEnumerator TimerRoutine()
    {
        // Use realtime so it still works even if someone pauses Time.timeScale later.
        while (_remaining > 0f)
        {
            _remaining -= uiTickInterval;
            if (_remaining < 0f) _remaining = 0f;

            UpdateTimerUI(_remaining);

            // Danger styling in last seconds
            if (_remaining <= redInLastSeconds)
                ApplyDangerStyle();
            else
                ApplyNormalStyle();

            yield return new WaitForSecondsRealtime(uiTickInterval);
        }

        _running = false;
        _timerRoutine = null;

        // finished
        if (showFailureWindowOnEnd)
            TriggerFailure(timeoutFailureMessage);
    }

    private void UpdateTimerUI(float seconds)
    {
        if (timerText == null) return;

        // Format: MM:SS (nice, readable)
        int sec = Mathf.CeilToInt(seconds);
        int minutes = sec / 60;
        int remainingSeconds = sec % 60;
        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }

    private void ApplyNormalStyle()
    {
        if (timerText != null) timerText.color = normalColor;
        if (timerLabelText != null) timerLabelText.color = normalColor;

        // Reset scale
        if (timerText != null) timerText.transform.localScale = _timerTextBaseScale;
        if (timerLabelText != null) timerLabelText.transform.localScale = _labelTextBaseScale;
    }

    private void ApplyDangerStyle()
    {
        if (timerText != null) timerText.color = dangerColor;
        if (timerLabelText != null) timerLabelText.color = dangerColor;

        if (!pulseInDanger) return;

        // Simple pulse (works nicely in VR)
        float t = Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed));
        float scale = Mathf.Lerp(pulseMinScale, 1f, t);

        if (timerText != null) timerText.transform.localScale = _timerTextBaseScale * scale;
        if (timerLabelText != null) timerLabelText.transform.localScale = _labelTextBaseScale * scale;
    }

    private void SetTimerUIVisible(bool visible)
    {
        if (timerText != null) timerText.gameObject.SetActive(visible);
        if (timerLabelText != null) timerLabelText.gameObject.SetActive(visible);
    }
}
