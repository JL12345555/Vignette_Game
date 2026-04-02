using UnityEngine;

public class HideAreaTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        InteractableObject interactable = other.GetComponentInParent<InteractableObject>();

        if (interactable != null)
        {
            interactable.isHiddenUnderTable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableObject interactable = other.GetComponentInParent<InteractableObject>();

        if (interactable != null)
        {
            interactable.isHiddenUnderTable = false;
        }
    }
}