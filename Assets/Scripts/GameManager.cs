using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [Header("Start Panel")]
    public GameObject startPanel;
    public TextMeshProUGUI startCountdownText;
    public float startPanelDuration = 10f;
    private float currentStartPanelTime;
    public bool isStartPanelActive = true;

    [Header("Timer")]
    public float roundTime = 60f;
    private float currentTime;
    public bool isTimeUp = false;

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public Slider attentionSlider;
    public TextMeshProUGUI timerText;

    [Header("Time Up UI")]
    public GameObject endPanel;
    public TextMeshProUGUI endText;

    [Header("Game Over UI")]
    public TextMeshProUGUI gameOverText;
    [TextArea] public string gameOverMessage = "Game Over";

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

        if (gameOverText != null)
        gameOverText.gameObject.SetActive(false);
    }

    void Start()
    {
        Time.timeScale = 1f;

        currentTime = roundTime;
        currentStartPanelTime = startPanelDuration;

        if (endPanel != null)
            endPanel.SetActive(false);


        if (startPanel != null)
            startPanel.SetActive(true);

        isStartPanelActive = true;
        Time.timeScale = 0f;

        UpdateUI();
        UpdateStartPanelUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }

        if (isStartPanelActive)
        {
            HandleStartPanel();
            return;
        }

        if (isGameOver || isTimeUp) return;

        HandleAttentionDecay();
        HandleTimer();
    }

    public void AddScore(float amount)
    {
        if (isGameOver || isTimeUp || isStartPanelActive) return;

        score += amount;
        UpdateUI();
    }

    public void AddTeacherAttention(float amount)
    {
        if (isGameOver || isTimeUp || isStartPanelActive) return;

        teacherAttention += amount;
        teacherAttention = Mathf.Clamp(teacherAttention, 0f, maxTeacherAttention);

        CheckAttentionGameOver();
        UpdateUI();
    }

    public void AddInteractionValues(float scoreAmount, float attentionAmount)
    {
        if (isGameOver || isTimeUp || isStartPanelActive) return;

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

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void HandleStartPanel()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndStartPanel();
            return;
        }

        currentStartPanelTime -= Time.unscaledDeltaTime;
        currentStartPanelTime = Mathf.Max(currentStartPanelTime, 0f);

        UpdateStartPanelUI();

        if (currentStartPanelTime <= 0f)
        {
            EndStartPanel();
        }
    }

    void EndStartPanel()
    {
        if (!isStartPanelActive) return;

        isStartPanelActive = false;

        if (startPanel != null)
            startPanel.SetActive(false);

        Time.timeScale = 1f;

        if (teacherController != null)
        {
            teacherController.BeginTeacherRoutine();
        }
    }

    void UpdateStartPanelUI()
    {
        if (startCountdownText != null)
        {
            startCountdownText.text = "Class starts in: " + Mathf.CeilToInt(currentStartPanelTime) + "\nPress E to skip";
        }
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

    void HandleTimer()
    {
        currentTime -= Time.deltaTime;
        currentTime = Mathf.Max(currentTime, 0f);

        UpdateUI();

        if (currentTime <= 0f)
        {
            TimeUp();
        }
    }

    void TimeUp()
    {
        if (isTimeUp) return;

        isTimeUp = true;

        if (firstPersonControllerScript != null)
        {
            firstPersonControllerScript.enabled = false;
        }

        Time.timeScale = 0f;

        if (endPanel != null)
            endPanel.SetActive(true);

        if (endText != null)
            endText.text = GetEndMessageByScore();
    }

    string GetEndMessageByScore()
    {
        if (score < 100)
        {
            return "You are lame. Just listen to the class"
        }
        else if (score < 200)
        {
            return "Come on! Go harder"
        }
        else if (score < 300)
        {
            return "You are cooking."
        }
        else
        {
            return "You are the ultimate chamption of distraction"
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
        if (isGameOver || isTimeUp || isStartPanelActive) return;

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

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = gameOverMessage;
        }

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

        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.CeilToInt(currentTime).ToString();
        }
    }
}