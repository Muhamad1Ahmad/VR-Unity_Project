using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StepDialogueUI : MonoBehaviour
{
    [System.Serializable]
    public class StepMessage
    {
        [TextArea(2, 4)]
        public string text = "Step text...";
        [Min(0.1f)]
        public float duration = 3f;
    }

    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;   // DialoguePanel
    [SerializeField] private TMP_Text dialogueText;  // DialogueText

    [Header("Steps (Edit in Inspector)")]
    [SerializeField] private List<StepMessage> steps = new List<StepMessage>();

    [Header("Behavior")]
    [Tooltip("Hide panel on start.")]
    [SerializeField] private bool hideOnStart = true;

    [Tooltip("If true, showing a new step will replace the current one immediately.")]
    [SerializeField] private bool interruptCurrent = true;

    private Coroutine _routine;

    private void Awake()
    {
        if (hideOnStart && panelRoot != null)
            panelRoot.SetActive(false);
    }
    private void Start()
    {
        ShowStep(0);
    }

    // Call this from UnityEvents: ShowStep(0), ShowStep(1), ...
    public void ShowStep(int stepIndex)
    {
        if (steps == null || steps.Count == 0) return;
        if (stepIndex < 0 || stepIndex >= steps.Count) return;

        if (interruptCurrent && _routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(ShowRoutine(steps[stepIndex].text, steps[stepIndex].duration));
    }

    // Optional: call with custom text (not from list)
    public void ShowCustom(string text, float seconds)
    {
        if (interruptCurrent && _routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(ShowRoutine(text, seconds));
    }

    public void HideNow()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;

        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private IEnumerator ShowRoutine(string text, float seconds)
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        if (dialogueText != null) dialogueText.text = text;

        yield return new WaitForSecondsRealtime(seconds);

        if (panelRoot != null) panelRoot.SetActive(false);
        _routine = null;
    }
}
