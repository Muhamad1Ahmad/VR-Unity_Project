using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SequencePackageController : MonoBehaviour
{
    public enum TriggerMode
    {
        Manual,         // Only via public methods
        OnStart,        // Auto-run when the scene starts
        KeyDown,        // Run when a key is pressed
        CollisionEnter, // Run when this object collides with another
        TriggerEnter    // Run when this object's trigger is entered
    }

    [Serializable]
    public class ObjectSet
    {
        [Tooltip("Turn these ON when the package runs.")]
        public List<GameObject> turnOn = new();

        [Tooltip("Turn these OFF when the package runs.")]
        public List<GameObject> turnOff = new();

        [Tooltip("Toggle these when the package runs.")]
        public List<GameObject> toggle = new();

        [Header("Optional Auto-Revert")]
        [Tooltip("If true, after 'durationSeconds' it reverts what it changed (On<->Off, Toggle again).")]
        public bool autoRevert = false;

        [Min(0f)]
        [Tooltip("How long to wait before auto-revert (only if autoRevert is enabled).")]
        public float durationSeconds = 0f;
    }

    [Serializable]
    public class ColliderSet
    {
        [Tooltip("Enable these colliders when the package runs.")]
        public List<Collider> enable = new();

        [Tooltip("Disable these colliders when the package runs.")]
        public List<Collider> disable = new();

        [Header("Optional Auto-Revert")]
        public bool autoRevert = false;

        [Min(0f)]
        public float durationSeconds = 0f;
    }

    [Serializable]
    public class SoundSet
    {
        [Tooltip("If set, the clip(s) will play through this AudioSource (recommended).")]
        public AudioSource audioSource;

        [Tooltip("Play these clips (one-shot). If multiple, one is chosen randomly.")]
        public List<AudioClip> oneShots = new();

        [Range(0f, 1f)]
        [Tooltip("Volume for one-shot clips.")]
        public float volume = 1f;

        [Tooltip("If enabled, stop the AudioSource before playing.")]
        public bool stopSourceBeforePlay = false;

        [Header("Optional: Start/Stop looping source")]
        [Tooltip("If true, will set audioSource.loop = true and play it.")]
        public bool startLoopingSource = false;

        [Tooltip("If true, will stop the audioSource.")]
        public bool stopSource = false;
    }

    [Serializable]
    public class TriggerFilter
    {
        [Tooltip("Optional: only trigger when the other object has this tag (leave empty = ignore).")]
        public string requiredTag = "";

        [Tooltip("Optional: only trigger when the other object is in this LayerMask.")]
        public LayerMask requiredLayers = ~0;

        [Tooltip("Optional: only trigger when the other collider belongs to this specific transform (leave null = ignore).")]
        public Transform requiredOtherRoot;
    }

    [Serializable]
    public class Package
    {
        [Header("Identity")]
        public string name = "New Package";
        public bool enabled = true;

        [Header("How this package is triggered")]
        public TriggerMode triggerMode = TriggerMode.Manual;

        [Tooltip("Used when TriggerMode = KeyDown.")]
        public KeyCode key = KeyCode.None;

        [Tooltip("Used when TriggerMode = CollisionEnter / TriggerEnter.")]
        public TriggerFilter filter = new();

        [Header("Timing")]
        [Min(0f)]
        public float delaySeconds = 0f;

        [Tooltip("If true, this package can only run once (per Play session).")]
        public bool runOnce = true;

        [Tooltip("If true, this package repeats forever with the same delay.")]
        public bool repeat = false;

        [Header("Actions")]
        public ObjectSet objects = new();
        public ColliderSet colliders = new();
        public SoundSet sounds = new();

        [Header("Optional Hooks")]
        public UnityEvent onStarted;
        public UnityEvent onExecuted;
        public UnityEvent onCompleted;
    }

    [Header("Packages (Sections)")]
    [SerializeField] private List<Package> packages = new();

    // Runtime
    private readonly Dictionary<int, Coroutine> _running = new();
    private readonly HashSet<int> _hasRun = new();

    private void Start()
    {
        // Auto-run any OnStart packages
        for (int i = 0; i < packages.Count; i++)
        {
            var p = packages[i];
            if (!p.enabled) continue;
            if (p.triggerMode == TriggerMode.OnStart)
                Trigger(i);
        }
    }

    private void Update()
    {
        // KeyDown triggers
        for (int i = 0; i < packages.Count; i++)
        {
            var p = packages[i];
            if (!p.enabled) continue;
            if (p.triggerMode != TriggerMode.KeyDown) continue;
            if (p.key == KeyCode.None) continue;

            if (Input.GetKeyDown(p.key))
                Trigger(i);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryTriggerByPhysicsEvent(TriggerMode.CollisionEnter, collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryTriggerByPhysicsEvent(TriggerMode.TriggerEnter, other);
    }

    private void TryTriggerByPhysicsEvent(TriggerMode mode, Collider other)
    {
        for (int i = 0; i < packages.Count; i++)
        {
            var p = packages[i];
            if (!p.enabled) continue;
            if (p.triggerMode != mode) continue;

            if (!PassesFilter(p.filter, other))
                continue;

            Trigger(i);
        }
    }

    private static bool PassesFilter(TriggerFilter filter, Collider other)
    {
        if (other == null) return false;

        if (!string.IsNullOrWhiteSpace(filter.requiredTag) && !other.CompareTag(filter.requiredTag))
            return false;

        int otherLayerBit = 1 << other.gameObject.layer;
        if ((filter.requiredLayers.value & otherLayerBit) == 0)
            return false;

        if (filter.requiredOtherRoot != null)
        {
            // Accept if the collider is under that root
            if (!other.transform.IsChildOf(filter.requiredOtherRoot) && other.transform != filter.requiredOtherRoot)
                return false;
        }

        return true;
    }

    // ----------------------------
    // Public API (UI Buttons etc.)
    // ----------------------------

    /// <summary>Trigger package by index (0-based).</summary>
    public void Trigger(int index)
    {
        if (index < 0 || index >= packages.Count) return;

        var p = packages[index];
        if (!p.enabled) return;

        if (p.runOnce && _hasRun.Contains(index))
            return;

        // If already running and it's repeat/once, ignore (prevents duplicates)
        if (_running.ContainsKey(index))
            return;

        _running[index] = StartCoroutine(RunPackage(index));
    }

    /// <summary>Trigger package by name (exact match).</summary>
    public void Trigger(string packageName)
    {
        if (string.IsNullOrWhiteSpace(packageName)) return;

        for (int i = 0; i < packages.Count; i++)
        {
            if (packages[i] != null && packages[i].name == packageName)
            {
                Trigger(i);
                return;
            }
        }
    }

    /// <summary>Stop a running package (if running).</summary>
    public void StopPackage(int index)
    {
        if (_running.TryGetValue(index, out var co) && co != null)
            StopCoroutine(co);

        _running.Remove(index);
    }

    /// <summary>Stop all running packages.</summary>
    public void StopAllPackages()
    {
        foreach (var kvp in _running)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        _running.Clear();
    }

    // ----------------------------
    // Internals
    // ----------------------------

    private IEnumerator RunPackage(int index)
    {
        var p = packages[index];
        p.onStarted?.Invoke();

        do
        {
            if (p.delaySeconds > 0f)
                yield return new WaitForSeconds(p.delaySeconds);

            ExecutePackageActions(p);

            p.onExecuted?.Invoke();

            // Mark as run (after execution)
            _hasRun.Add(index);

            // Optional completion hook (after any auto-reverts finish)
            yield return HandleAutoReverts(p);

            p.onCompleted?.Invoke();

            // If repeat is enabled, loop again. Otherwise break.
        }
        while (p.repeat);

        _running.Remove(index);
    }

    private static void ExecutePackageActions(Package p)
    {
        // --- Sounds ---
        if (p.sounds != null && p.sounds.audioSource != null)
        {
            if (p.sounds.stopSource)
                p.sounds.audioSource.Stop();

            if (p.sounds.startLoopingSource)
            {
                p.sounds.audioSource.loop = true;
                if (p.sounds.stopSourceBeforePlay) p.sounds.audioSource.Stop();
                p.sounds.audioSource.Play();
            }
            else if (p.sounds.oneShots != null && p.sounds.oneShots.Count > 0)
            {
                if (p.sounds.stopSourceBeforePlay) p.sounds.audioSource.Stop();
                var clip = p.sounds.oneShots[UnityEngine.Random.Range(0, p.sounds.oneShots.Count)];
                if (clip != null) p.sounds.audioSource.PlayOneShot(clip, p.sounds.volume);
            }
        }

        // --- Colliders ---
        if (p.colliders != null)
        {
            SetEnabled(p.colliders.enable, true);
            SetEnabled(p.colliders.disable, false);
        }

        // --- Objects ---
        if (p.objects != null)
        {
            SetActive(p.objects.turnOn, true);
            SetActive(p.objects.turnOff, false);
            Toggle(p.objects.toggle);
        }
    }

    private static void SetActive(List<GameObject> list, bool value)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (go != null) go.SetActive(value);
        }
    }

    private static void Toggle(List<GameObject> list)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            var go = list[i];
            if (go != null) go.SetActive(!go.activeSelf);
        }
    }

    private static void SetEnabled(List<Collider> list, bool value)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; i++)
        {
            var c = list[i];
            if (c != null) c.enabled = value;
        }
    }

    private static IEnumerator HandleAutoReverts(Package p)
    {
        float maxWait = 0f;

        if (p.objects != null && p.objects.autoRevert)
            maxWait = Mathf.Max(maxWait, p.objects.durationSeconds);

        if (p.colliders != null && p.colliders.autoRevert)
            maxWait = Mathf.Max(maxWait, p.colliders.durationSeconds);

        // Nothing to revert
        if (maxWait <= 0f)
            yield break;

        // We do independent waits (so objects and colliders can have different durations)
        if (p.objects != null && p.objects.autoRevert && p.objects.durationSeconds > 0f)
        {
            yield return new WaitForSeconds(p.objects.durationSeconds);

            // Revert objects: On<->Off and toggle again
            SetActive(p.objects.turnOn, false);
            SetActive(p.objects.turnOff, true);
            Toggle(p.objects.toggle);
        }

        if (p.colliders != null && p.colliders.autoRevert && p.colliders.durationSeconds > 0f)
        {
            yield return new WaitForSeconds(p.colliders.durationSeconds);

            // Revert colliders: enable<->disable
            SetEnabled(p.colliders.enable, false);
            SetEnabled(p.colliders.disable, true);
        }
    }
}
