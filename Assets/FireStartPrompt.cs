using UnityEngine;
using System.Collections;

public class FireStartPrompt : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private StepDialogueUI dialogueUI;
    [SerializeField] private AlarmCountdownGameManager gameManager;

    [SerializeField] private GameObject failedWindow;

    [Header("Visual Sync")]
    [Tooltip("Drag the main fire particle system here.")]
    [SerializeField] private ParticleSystem fireParticles;
    
    [Tooltip("Add extra seconds after the fire is fully visible?")]
    [SerializeField] private float bufferTime = 1.0f;

    [Header("Message")]
    [TextArea] 
    [SerializeField] private string message = "Press the Fire Alarm Button!";
    [SerializeField] private float showDuration = 4.0f;

    private void Start()
    {
        float waitTime = CalculateWaitTime();
        StartCoroutine(CheckAndShowMessage(waitTime));
    }

    private float CalculateWaitTime()
    {
        // Default to 3 seconds if no particle system is assigned
        if (fireParticles == null) return 3.0f;

        // Get the lifetime (how long it takes to reach 'steady state')
        // We use constantMax to get the longest possible lifetime if it's a range.
        float lifetime = fireParticles.main.startLifetime.constantMax;
        
        // Check if there is a start delay (time before it even starts emitting)
        float startDelay = fireParticles.main.startDelay.constantMax;

        return startDelay + lifetime + bufferTime;
    }

    private IEnumerator CheckAndShowMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (failedWindow != null && failedWindow.activeInHierarchy) yield break;

        // Check logic (same as before)
        bool alarmAlreadyActive = false;
        if (gameManager != null)
        {
            alarmAlreadyActive = gameManager.IsAlarmRunning();
        }

        if (!alarmAlreadyActive && dialogueUI != null)
        {
            dialogueUI.ShowCustom(message, showDuration);
        }
    }
}