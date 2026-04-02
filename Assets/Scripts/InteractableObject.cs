using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Prompt")]
    [TextArea]
    public string promptText = "Interact";

    [Header("Score / Attention Per Tick")]
    public float scorePerTick = 1f;
    public float attentionPerTick = 1f;

    [Header("Hide Under Table")]
    public bool isHiddenUnderTable = false;
    public float hiddenAttentionMultiplier = 0.5f;

    [Header("Loop Sound")]
    public AudioSource loopAudioSource;

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
        isHiddenUnderTable = false;
        StopLoopSound();

    }

    public float GetCurrentAttentionPerTick()
    {
        if (isHiddenUnderTable)
        {
            return attentionPerTick * hiddenAttentionMultiplier;
        }

        return attentionPerTick;
    }

    public void PlayLoopSound()
    {
        if (loopAudioSource == null) return;

        loopAudioSource.loop = true;

        if (!loopAudioSource.isPlaying)
        {
            loopAudioSource.Play();
        }
    }

    public void StopLoopSound()
    {
        if (loopAudioSource == null) return;

        if (loopAudioSource.isPlaying)
        {
            loopAudioSource.Stop();
        }
    }
}