using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Stats")]
    public float score = 0f;
    public float teacherAttention = 0f;
    public float maxTeacherAttention = 100f;

    [Header("Attention Decay")]
    public float attentionDecayAmount = 5f;
    public float attentionDecayInterval = 0.5f;

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public Slider attentionSlider;

    [Header("References")]
    public TeacherController teacherController;
    public Transform playerCameraTransform;
    public MonoBehaviour firstPersonControllerScript;
    public InteractionController interactionController;

    [Header("Game Over Camera")]
    public Vector3 gameOverCameraEuler = new Vector3(0f, 10f, 0f);

    private float attentionDecayTimer = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (isGameOver) return;

        HandleAttentionDecay();
    }

    public void AddScore(float amount)
    {
        if (isGameOver) return;

        score += amount;
        UpdateUI();
    }

    public void AddTeacherAttention(float amount)
    {
        if (isGameOver) return;

        teacherAttention += amount;
        teacherAttention = Mathf.Clamp(teacherAttention, 0f, maxTeacherAttention);

        CheckAttentionGameOver();
        UpdateUI();
    }

    public void AddInteractionValues(float scoreAmount, float attentionAmount)
    {
        if (isGameOver) return;

        score += scoreAmount;
        teacherAttention += attentionAmount;
        teacherAttention = Mathf.Clamp(teacherAttention, 0f, maxTeacherAttention);

        CheckAttentionGameOver();
        UpdateUI();
    }

    public bool PlayerIsHoldingObject()
    {
        if (interactionController == null) return false;
        return interactionController.IsHoldingObject();
    }

    void HandleAttentionDecay()
    {
        if (PlayerIsHoldingObject())
        {
            attentionDecayTimer = 0f;
            return;
        }

        attentionDecayTimer += Time.deltaTime;

        if (attentionDecayTimer >= attentionDecayInterval)
        {
            teacherAttention -= attentionDecayAmount;
            teacherAttention = Mathf.Clamp(teacherAttention, 0f, maxTeacherAttention);

            attentionDecayTimer = 0f;
            UpdateUI();
        }
    }

    void CheckAttentionGameOver()
    {
        if (teacherAttention >= maxTeacherAttention)
        {
            TriggerGameOver();
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (firstPersonControllerScript != null)
        {
            firstPersonControllerScript.enabled = false;
        }

        if (playerCameraTransform != null)
        {
            playerCameraTransform.localRotation = Quaternion.Euler(gameOverCameraEuler);
        }

        if (teacherController != null)
        {
            teacherController.PlayCatchSequence();
        }

        Debug.Log("Game Over");
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(score).ToString();
        }

        if (attentionSlider != null)
        {
            attentionSlider.maxValue = maxTeacherAttention;
            attentionSlider.value = teacherAttention;
        }
    }
}