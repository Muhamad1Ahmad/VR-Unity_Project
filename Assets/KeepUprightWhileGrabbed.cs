using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class KeepUprightWhileGrabbed : MonoBehaviour
{
    XRGrabInteractable grab;
    bool held;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(_ => held = true);
        grab.selectExited.AddListener(_ => held = false);
    }

    void LateUpdate()
    {
        if (!held) return;

        // Keep only yaw, remove pitch/roll
        var e = transform.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, e.y, 0f);
    }
}
