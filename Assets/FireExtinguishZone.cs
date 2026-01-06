using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguishZone : MonoBehaviour
{
    [Header("--- DETECTION TAGS ---")]
    [Tooltip("Tag for the CORRECT extinguisher (e.g. 'FumesClassD').")]
    [SerializeField] private string correctFumesTag = "FumesClassD";

    [Tooltip("Tag for the WRONG extinguisher (e.g. 'FumesWater').")]
    [SerializeField] private string wrongFumesTag = "FumesWater";

    [Header("--- SUCCESS SETTINGS ---")]
    [Tooltip("How long you must spray to put out the fire.")]
    [SerializeField] private float secondsToExtinguish = 2.0f;

    [Tooltip("Object to turn on SLOWLY (e.g., 'Success' 3D Text).")]
    [SerializeField] private GameObject successObjectSlow;
    [SerializeField] private float successAppearDuration = 1.5f;

    [Tooltip("List of objects to turn ON immediately when extinguished.")]
    [SerializeField] private List<GameObject> turnOnWhenExtinguished = new List<GameObject>();

    [Tooltip("List of objects to turn OFF immediately when extinguished.")]
    [SerializeField] private List<GameObject> turnOffWhenExtinguished = new List<GameObject>();

    [Header("--- FAILURE SETTINGS (Wrong Fumes) ---")]
    [Tooltip("How much bigger the fire gets when hit by wrong fumes (e.g., 1.5x).")]
    [SerializeField] private float fireBoostMultiplier = 1.5f;

    [Tooltip("Object to show TEMPORARILY (e.g., 'Wrong Extinguisher' Warning).")]
    [SerializeField] private GameObject wrongFumesWarningObject;
    [SerializeField] private float warningDuration = 3.0f;

    [Tooltip("Sound to play when fire worsens.")]
    [SerializeField] private AudioSource failureAudio;

    [Header("--- FIRE VFX CONTROL ---")]
    [Tooltip("All fire particle systems to stop/boost.")]
    [SerializeField] private List<ParticleSystem> fireParticles = new List<ParticleSystem>();

    [Tooltip("Root object to disable after fire is out.")]
    [SerializeField] private GameObject fireRootToDisable;

    [Tooltip("Delay before disabling the fire root (lets smoke fade out).")]
    [SerializeField] private float disableRootDelay = 2.0f;

    [Header("--- FAILURE UI (Game Over) ---")]
    [Tooltip("The entire Window object (Failed_Window) to show when the user fails.")]
    public GameObject failedWindow;

    [Tooltip("The Text object inside the window (to change the reason dynamically).")]
    public TMPro.TMP_Text failureReasonText;
    
    public GameObject locomotionSystem; // Drag your 'XR Origin' or 'Locomotion System' here

    // --- Internal State ---
    private int _correctFumesInsideCount = 0;
    private float _progress = 0f;
    private bool _extinguished = false;
    private bool _failed = false; // If true, fire is currently "boosted"
    private bool _warningActive = false;
    private bool _isGameOver = false; // Tracks if the game has ended

    // Store original values for Reset
    private Vector3 _successOriginalScale;
    private Vector3 _fireOriginalScale;
    private List<float> _originalEmissionRates = new List<float>();
    private List<float> _originalStartSizes = new List<float>();

    private void Awake()
    {
        // 1. Store Original Scales
        _fireOriginalScale = transform.localScale;

        if (successObjectSlow != null)
        {
            _successOriginalScale = successObjectSlow.transform.localScale;
            successObjectSlow.SetActive(false);
        }

        if (wrongFumesWarningObject != null)
            wrongFumesWarningObject.SetActive(false);

        // 2. Store Particle Original Values (for Reset)
        foreach (var ps in fireParticles)
        {
            if (ps != null)
            {
                _originalStartSizes.Add(ps.main.startSizeMultiplier);
                _originalEmissionRates.Add(ps.emission.rateOverTimeMultiplier);
            }
        }
    }

    private void Update()
    {
        if (_extinguished) return;

        // Logic: Only make progress if CORRECT fumes are hitting it
        bool sprayingCorrectly = _correctFumesInsideCount > 0;

        if (sprayingCorrectly)
        {
            _progress += Time.deltaTime;

            // Check if done
            if (_progress >= secondsToExtinguish)
            {
                StartCoroutine(ExtinguishRoutine());
            }
        }
        else
        {
            // Decay progress if they stop spraying
            _progress = Mathf.Max(0f, _progress - 0.5f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the game is already over (failed or won), ignore everything.
        if (_isGameOver) return;
        
        if (_extinguished) return;

        // --- WRONG FUMES LOGIC ---
        if (other.CompareTag(wrongFumesTag))
        {
            // 1. Show Warning
            if (!_warningActive) StartCoroutine(ShowWrongWarningRoutine());

            // 2. Boost Fire (Only boost if not already failed/boosted to avoid infinite growth)
            if (!_failed)
            {
                BoostFire();
            }
        }
        // --- CORRECT FUMES LOGIC ---
        else if (other.CompareTag(correctFumesTag))
        {
            _correctFumesInsideCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(correctFumesTag))
        {
            _correctFumesInsideCount = Mathf.Max(0, _correctFumesInsideCount - 1);
        }
    }

    // ---------------------------------------------
    // LOGIC FUNCTIONS
    // ---------------------------------------------

    private void BoostFire()
    {
        _failed = true;
        _isGameOver = true;
        Debug.Log("WRONG EXTINGUISHER! Fire Boosted!");

        // 1. Play Sound
        if (failureAudio != null) failureAudio.Play();

        // 2. Scale Up the whole fire area
        transform.localScale = _fireOriginalScale * fireBoostMultiplier;

        // 3. Boost Particles (Emission & Size)
        foreach (var ps in fireParticles)
        {
            if (ps == null) continue;

            var main = ps.main;
            main.startSizeMultiplier *= fireBoostMultiplier;

            var emission = ps.emission;
            emission.rateOverTimeMultiplier *= fireBoostMultiplier;
        }

        // 4. SHOW FAILURE WINDOW (New)
        if (failedWindow != null) 
        {
            failedWindow.SetActive(true);
            
            // Optional: Set specific text for this failure
            if (failureReasonText != null) 
                failureReasonText.text = "WRONG EXTINGUISHER!\nFire Intensified.";
        }

        // 5. DISABLE PLAYER MOVEMENT
        // This keeps them stuck in place looking at the failure message.
        if (locomotionSystem != null)
        {
          locomotionSystem.SetActive(false);  
        } 
        
        // // 4. STOP THE SIMULATION (New)
        // Time.timeScale = 0f; // Pauses the game loop
    }

    private IEnumerator ShowWrongWarningRoutine()
    {
        _warningActive = true;
        if (wrongFumesWarningObject != null) wrongFumesWarningObject.SetActive(true);

        yield return new WaitForSeconds(warningDuration);

        if (wrongFumesWarningObject != null) wrongFumesWarningObject.SetActive(false);
        _warningActive = false;
    }

    private IEnumerator ExtinguishRoutine()
    {
        _extinguished = true;

        // 1. Stop Fire Particles
        foreach (var ps in fireParticles)
        {
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 2. Handle Generic Lists (Instant Turn On/Off)
        foreach (var obj in turnOnWhenExtinguished)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in turnOffWhenExtinguished)
            if (obj != null) obj.SetActive(false);

        // 3. Handle Slow Success Object (Animation)
        if (successObjectSlow != null)
        {
            successObjectSlow.SetActive(true);
            successObjectSlow.transform.localScale = Vector3.zero;

            float timer = 0f;
            while (timer < successAppearDuration)
            {
                timer += Time.deltaTime;
                float percent = timer / successAppearDuration;
                successObjectSlow.transform.localScale = Vector3.Lerp(Vector3.zero, _successOriginalScale, Mathf.SmoothStep(0f, 1f, percent));
                yield return null;
            }
            successObjectSlow.transform.localScale = _successOriginalScale;
        }

        // 4. Disable Root (after smoke fades)
        if (disableRootDelay > 0f) yield return new WaitForSeconds(disableRootDelay);
        if (fireRootToDisable != null) fireRootToDisable.SetActive(false);
    }

    // ---------------------------------------------
    // RESET FUNCTION (Call from UI Button)
    // ---------------------------------------------
    public void ResetSimulation()
    {
        StopAllCoroutines();

        _isGameOver = false;
        _extinguished = false;
        _failed = false;
        _progress = 0f;
        _correctFumesInsideCount = 0;
        _warningActive = false;

        if (locomotionSystem != null) locomotionSystem.SetActive(true);

        // 2. Hide Failure Window
        if (failedWindow != null) failedWindow.SetActive(false);

        // 3. Reset Fire Root & Scale
        if (fireRootToDisable != null) fireRootToDisable.SetActive(true);
        transform.localScale = _fireOriginalScale;

        // 2. Reset Particles (Play + Reset Values)
        for (int i = 0; i < fireParticles.Count; i++)
        {
            var ps = fireParticles[i];
            if (ps != null)
            {
                // Reset to original values stored in Awake
                if (i < _originalStartSizes.Count)
                {
                    var main = ps.main;
                    main.startSizeMultiplier = _originalStartSizes[i];

                    var emission = ps.emission;
                    emission.rateOverTimeMultiplier = _originalEmissionRates[i];
                }
                ps.Play();
            }
        }

        // 5. Reset Success/Fail Objects
        if (successObjectSlow != null) successObjectSlow.SetActive(false);
        if (wrongFumesWarningObject != null) wrongFumesWarningObject.SetActive(false);

        // 6. Reset Generic Lists
        foreach (var obj in turnOnWhenExtinguished)
            if (obj != null) obj.SetActive(false); // Turn them back OFF

        foreach (var obj in turnOffWhenExtinguished)
            if (obj != null) obj.SetActive(true);  // Turn them back ON

        Debug.Log("Simulation Reset!");
    }
}