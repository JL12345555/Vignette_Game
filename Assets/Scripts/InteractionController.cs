using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public TextMeshProUGUI promptUI;
    public Transform holdPoint;

    [Header("Raycast Settings")]
    public float rayDistance = 4f;
    public LayerMask interactLayer;

    [Header("Hold Settings")]
    public float moveSpeed = 10f;

    [Header("Tutorial")]
    public string firstLookText = "Press E";
    public float firstLookDuration = 3f;

    [Header("Interaction Tick")]
    public float interactionTickRate = 0.5f;

    private static bool hasShownFirstLookPrompt = false;

    private InteractableObject currentTarget;
    private InteractableObject heldObject;

    private bool isShowingFirstLook = false;
    private float firstLookTimer = 0f;

    private Coroutine interactionCoroutine;

    void Start()
    {
        if (promptUI != null)
            promptUI.gameObject.SetActive(false);
    }

    void Update()
    {
        DetectObject();
        HandleInteraction();
        MoveHeldObject();
        UpdateFirstLookTimer();
    }

    void DetectObject()
    {
        if (heldObject != null)
        {
            currentTarget = null;
            HidePrompt();
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactLayer))
        {
            InteractableObject newTarget = hit.collider.GetComponentInParent<InteractableObject>();

            if (newTarget != null)
            {
                currentTarget = newTarget;
                ShowPrompt();
                return;
            }
        }

        currentTarget = null;
        HidePrompt();
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null && currentTarget != null)
            {
                PickUpObject(currentTarget);
            }
            else if (heldObject != null)
            {
                DropObject();
            }
        }
    }

    void PickUpObject(InteractableObject obj)
    {
        heldObject = obj;
        heldObject.SaveOriginalState();
        heldObject.isBeingHeld = true;

        heldObject.transform.SetParent(null);

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        StartInteractionTick(heldObject);
        HidePrompt();
    }

    void DropObject()
    {
        if (heldObject == null) return;

        StopInteractionTick();

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        heldObject.ReturnToOriginalState();
        heldObject = null;
    }

    void MoveHeldObject()
    {
        if (heldObject == null || holdPoint == null) return;

        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position,
            holdPoint.position,
            moveSpeed * Time.deltaTime
        );

        heldObject.transform.rotation = Quaternion.Lerp(
            heldObject.transform.rotation,
            holdPoint.rotation,
            moveSpeed * Time.deltaTime
        );
    }

    void StartInteractionTick(InteractableObject obj)
    {
        StopInteractionTick();
        interactionCoroutine = StartCoroutine(InteractionTickRoutine(obj));
    }

    void StopInteractionTick()
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(interactionCoroutine);
            interactionCoroutine = null;
        }
    }

    IEnumerator InteractionTickRoutine(InteractableObject obj)
    {
        while (heldObject == obj && obj != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddInteractionValues(
                    obj.scorePerTick,
                    obj.attentionPerTick
                );
            }

            yield return new WaitForSeconds(interactionTickRate);
        }
    }

    void ShowPrompt()
    {
        if (promptUI == null || currentTarget == null) return;

        promptUI.gameObject.SetActive(true);

        if (!hasShownFirstLookPrompt)
        {
            promptUI.text = firstLookText;
            hasShownFirstLookPrompt = true;
            isShowingFirstLook = true;
            firstLookTimer = firstLookDuration;
        }
        else if (isShowingFirstLook)
        {
            promptUI.text = firstLookText;
        }
        else
        {
            promptUI.text = currentTarget.promptText;
        }
    }

    void UpdateFirstLookTimer()
    {
        if (!isShowingFirstLook) return;

        firstLookTimer -= Time.deltaTime;

        if (firstLookTimer <= 0f)
        {
            isShowingFirstLook = false;

            if (currentTarget != null && promptUI != null)
            {
                promptUI.text = currentTarget.promptText;
            }
        }
    }

    void HidePrompt()
    {
        if (promptUI != null)
            promptUI.gameObject.SetActive(false);
    }
}