using UnityEngine;
using System.Collections;

public class GestureStabilizer : MonoBehaviour
{
    [Tooltip("The object you want to toggle (e.g., Teleport Interactor)")]
    public GameObject targetObject;

    [Tooltip("How long to keep the object active after the gesture is lost (in seconds).")]
    public float lossDelay = 1.0f; // 1 second is plenty of time for the simulator

    private Coroutine _deactivateRoutine;

    // Call this from "Gesture Performed"
    public void OnGestureFound()
    {
        // 1. Cancel any pending turn-off command
        if (_deactivateRoutine != null)
        {
            StopCoroutine(_deactivateRoutine);
            _deactivateRoutine = null;
        }

        // 2. Turn the object ON immediately
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    // Call this from "Gesture Ended"
    public void OnGestureLost()
    {
        // Don't turn it off yet! Wait for the delay.
        if (gameObject.activeInHierarchy)
        {
            _deactivateRoutine = StartCoroutine(WaitAndDeactivate());
        }
    }

    private IEnumerator WaitAndDeactivate()
    {
        yield return new WaitForSeconds(lossDelay);

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        _deactivateRoutine = null;
    }
}