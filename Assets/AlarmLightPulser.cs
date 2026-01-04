using UnityEngine;

public class AlarmLightPulser : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("The lowest brightness value.")]
    public float minIntensity = 0f;

    [Tooltip("The highest brightness value.")]
    public float maxIntensity = 5f;

    [Tooltip("How fast the light pulses (higher = faster).")]
    public float pulseSpeed = 2f;

    [Header("Optional Color")]
    [Tooltip("If checked, it will also force the lights to Red (or your chosen color).")]
    public bool forceColor = true;
    public Color alarmColor = Color.red;

    // Array to store all the lights found in the children
    private Light[] _childLights;

    void Start()
    {
        // 1. Find all Light components attached to this object AND its children
        _childLights = GetComponentsInChildren<Light>();

        // 2. (Optional) Set their color immediately so they match
        if (forceColor)
        {
            foreach (Light l in _childLights)
            {
                l.color = alarmColor;
            }
        }
    }

    void Update()
    {
        if (_childLights.Length == 0) return;

        // 3. Calculate the new intensity (PingPong moves values back and forth like a wave)
        // Time.time * speed controls the rate. The '1' means the value goes from 0 to 1.
        float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);

        // 4. Smoothly blend between Min and Max based on 't'
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, t);

        // 5. Apply to all lights
        foreach (Light l in _childLights)
        {
            l.intensity = currentIntensity;
        }
    }
}