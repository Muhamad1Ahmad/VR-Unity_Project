using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; 

public class ExtinguisherTutorial : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private StepDialogueUI dialogueUI;

    [Header("Message")]
    [TextArea]
    [SerializeField] private string howToUseMessage = "Aim at the base of the fire and press the TRIGGER to spray!";
    [SerializeField] private float messageDuration = 6.0f;

    // FIX: Use the standard class name without extra namespaces
    private XRGrabInteractable _grabInteractable;
    private bool _hasPickedUp = false;

    private void Awake()
    {
        // FIX: Standard GetComponent call
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        if (_grabInteractable != null)
            _grabInteractable.selectEntered.AddListener(OnPickedUp);
    }

    private void OnDisable()
    {
        if (_grabInteractable != null)
            _grabInteractable.selectEntered.RemoveListener(OnPickedUp);
    }

    // FIX: Use 'SelectEnterEventArgs' which works in XR Toolkit 2.x and 3.x
    private void OnPickedUp(SelectEnterEventArgs args)
    {
        if (!_hasPickedUp)
        {
            _hasPickedUp = true;
            if (dialogueUI != null)
            {
                dialogueUI.ShowCustom(howToUseMessage, messageDuration);
            }
        }
    }
}