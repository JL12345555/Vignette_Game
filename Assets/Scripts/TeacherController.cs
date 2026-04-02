using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class TeacherController : MonoBehaviour
{
    public enum TeacherState
    {
        Talking,
        Patrolling,
        Returning,
        GameOver
    }

    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform patrolRouteRoot;

    [Header("Talk Timing")]
    public float minTalkTime = 15f;
    public float maxTalkTime = 20f;

    [Header("Patrol Settings")]
    public float pointReachedDistance = 0.3f;

    [Header("Game Over")]
    public Transform catchPoint;

    [Header("Audio")]
    public AudioSource talkingAudioSource;
    public AudioSource flyKickAudioSource;

    private Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private TeacherState currentState;
    private Coroutine talkCoroutine;
    private bool hasStartedRoutine = false;

    void Start()
    {
        SetupPatrolPoints();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points found.");
            enabled = false;
            return;
        }

        transform.position = patrolPoints[0].position;
        transform.rotation = patrolPoints[0].rotation;

        StopTalkingSound();
        SetAnimatorState(false, false);
    }

    void Update()
    {
        if (!hasStartedRoutine) return;

        if (GameManager.Instance != null && GameManager.Instance.isGameOver)
            return;

        if (currentState == TeacherState.Patrolling)
        {
            CheckPatrolProgress();
        }
    }

    public void BeginTeacherRoutine()
    {
        if (hasStartedRoutine) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        hasStartedRoutine = true;
        EnterTalkingState();
    }

    void SetupPatrolPoints()
    {
        if (patrolRouteRoot == null)
        {
            Debug.LogError("Patrol Route Root is missing.");
            patrolPoints = new Transform[0];
            return;
        }

        int childCount = patrolRouteRoot.childCount;
        patrolPoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            patrolPoints[i] = patrolRouteRoot.GetChild(i);
        }
    }

    void EnterTalkingState()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        currentState = TeacherState.Talking;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        SetAnimatorState(true, false);
        transform.rotation = patrolPoints[0].rotation;

        PlayTalkingSound();

        if (talkCoroutine != null)
            StopCoroutine(talkCoroutine);

        talkCoroutine = StartCoroutine(TalkThenPatrolRoutine());
    }

    IEnumerator TalkThenPatrolRoutine()
    {
        float talkTime = Random.Range(minTalkTime, maxTalkTime);
        yield return new WaitForSeconds(talkTime);

        StartPatrol();
    }

    void StartPatrol()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        if (patrolPoints.Length < 2)
        {
            Debug.LogWarning("Need at least 2 patrol points for patrol.");
            EnterTalkingState();
            return;
        }

        currentState = TeacherState.Patrolling;
        currentPatrolIndex = 1;

        StopTalkingSound();

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }

        SetAnimatorState(false, true);
    }

    void CheckPatrolProgress()
    {
        if (agent == null || agent.pathPending) return;

        if (agent.remainingDistance <= pointReachedDistance)
        {
            if (currentPatrolIndex < patrolPoints.Length - 1)
            {
                currentPatrolIndex++;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            else
            {
                StartReturnToTalk();
            }
        }
    }

    void StartReturnToTalk()
    {
        if (currentState == TeacherState.Returning) return;
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        currentState = TeacherState.Returning;

        StopTalkingSound();

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolPoints[0].position);
        }

        SetAnimatorState(false, true);
        StartCoroutine(ReturnToTalkPointRoutine());
    }

    IEnumerator ReturnToTalkPointRoutine()
    {
        while (agent.pathPending || agent.remainingDistance > pointReachedDistance)
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver)
                yield break;

            yield return null;
        }

        transform.position = patrolPoints[0].position;
        transform.rotation = patrolPoints[0].rotation;

        EnterTalkingState();
    }

    public void PlayCatchSequence()
    {
        currentState = TeacherState.GameOver;

        if (talkCoroutine != null)
            StopCoroutine(talkCoroutine);

        StopAllCoroutines();
        StopTalkingSound();

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        if (catchPoint != null)
        {
            transform.position = catchPoint.position;
            transform.rotation = catchPoint.rotation;
        }

        SetAnimatorState(false, false);

        if (animator != null)
        {
            animator.SetTrigger("flykick");
        }

        PlayFlyKickSound();
    }

    void SetAnimatorState(bool isTalking, bool isWalking)
    {
        if (animator == null) return;

        animator.SetBool("isTalking", isTalking);
        animator.SetBool("isWalking", isWalking);
    }

    void PlayTalkingSound()
    {
        if (talkingAudioSource == null) return;

        talkingAudioSource.loop = true;

        if (!talkingAudioSource.isPlaying)
        {
            talkingAudioSource.Play();
        }
    }

    void StopTalkingSound()
    {
        if (talkingAudioSource == null) return;

        if (talkingAudioSource.isPlaying)
        {
            talkingAudioSource.Stop();
        }
    }

    void PlayFlyKickSound()
    {
        if (flyKickAudioSource == null) return;

        flyKickAudioSource.loop = false;
        flyKickAudioSource.Play();
    }

    void OnTriggerStay(Collider other)
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.isGameOver) return;

        if (!other.CompareTag("Player")) return;
        if (!GameManager.Instance.PlayerIsHoldingObject()) return;

        GameManager.Instance.TriggerGameOver();
    }
}