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

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public Slider attentionSlider;

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

    public void AddScore(float amount)
    {
        score += amount;
        UpdateUI();
    }

    public void AddTeacherAttention(float amount)
    {
        teacherAttention += amount;
        teacherAttention = Mathf.Clamp(teacherAttention, 0f, maxTeacherAttention);
        UpdateUI();
    }

    public void AddInteractionValues(float scoreAmount, float attentionAmount)
    {
        score += scoreAmount;
        teacherAttention += attentionAmount;
        teacherAttention = Mathf.Clamp(teacherAttention, 0f, maxTeacherAttention);
        UpdateUI();
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