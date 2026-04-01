using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Prompt")]
    [TextArea]
    public string promptText = "Interact";

    [Header("Score / Attention Per Tick")]
    public float scorePerTick = 1f;
    public float attentionPerTick = 1f;

    [HideInInspector] public Vector3 originalPosition;
    [HideInInspector] public Quaternion originalRotation;
    [HideInInspector] public Transform originalParent;

    [HideInInspector] public bool isBeingHeld = false;

    public void SaveOriginalState()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;
    }

    public void ReturnToOriginalState()
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        isBeingHeld = false;
    }
}