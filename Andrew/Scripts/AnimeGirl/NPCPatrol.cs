using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Patrol, Chase, Attack, Search }

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;
    private NavMeshAgent agent;
    private AudioSource audioSource;

    [Header("Vision Settings")]
    public float viewDistance = 12f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Hearing Settings")]
    public float hearingRadius = 10f;

    [Header("Behaviour Settings")]
    public float searchTime = 5f;
    public float lostSightTime = 3f;
    public float attackRange = 2f;      // расстояние для перехода в атаку
    public float attackCooldown = 2f;   // задержка между атаками

    [Header("Voice Lines")]
    public AudioClip[] heardClips;
    public AudioClip[] spottedClips;
    public AudioClip[] lostClips;
    public AudioClip[] attackClips;
    public AudioClip[] lostFinallyClips;

    public float minVoiceDelay = 5f;

    private NPCState currentState;
    private int currentPatrolIndex;
    private float stateTimer;
    private float lastVoiceTime;
    private float lastAttackTime;
    private Vector3 lastHeardPosition;
    private bool playerInSight;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        currentState = NPCState.Patrol;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCState.Patrol:
                Patrol();
                break;
            case NPCState.Chase:
                Chase();
                break;
            case NPCState.Attack:
                Attack();
                break;
            case NPCState.Search:
                Search();
                break;
        }

        DetectPlayer();
    }

    // ---------- PATROL ----------
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // ---------- CHASE ----------
    void Chase()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            currentState = NPCState.Attack;
            agent.isStopped = true;
            return;
        }

        if (playerInSight)
        {
            agent.isStopped = false;
            agent.destination = player.position;
            stateTimer = 0;
            animator.SetBool("slow running", true);
            animator.SetBool("walking", false);
            agent.speed = 2.5f;
        }
        else
        {
            stateTimer += Time.deltaTime;
            if (stateTimer >= lostSightTime)
            {
                PlayVoice(lostClips);
                currentState = NPCState.Search;
                stateTimer = 0;
                lastHeardPosition = player.position;
                agent.destination = lastHeardPosition;
                animator.SetBool("slow running", false);
                animator.SetBool("walking", true);
                agent.speed = 1.8f;
            }
        }
    }

    // ---------- ATTACK ----------
    void Attack()
    {
        if (player == null) return;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange * 1.2f)
        {
            currentState = NPCState.Chase;
            agent.isStopped = false;
            agent.speed = 2.5f;
            animator.SetBool("battle", false);
            animator.SetBool("slow running", true);
            animator.SetBool("walking", false);
            return;
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            agent.isStopped = true;
            lastAttackTime = Time.time;
            PlayVoice(attackClips);
            Debug.Log($"{name} атакует игрока!");
            // ЗДЕСЬ РЕАЛИЗОВАТЬ УРОН ПО ИГРОКУ 
            animator.SetBool("battle", true);
            agent.speed = 0f;
            animator.SetBool("slow running", false);
            animator.SetBool("walking", false);
        }
    }

    // ---------- SEARCH ----------
    void Search()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            stateTimer += Time.deltaTime;
            animator.SetBool("walking", false);
            animator.SetBool("slow running", false);
            animator.SetBool("walking", false);
            animator.SetBool("battle", false);
            animator.SetBool("lookAround", true);
            if (stateTimer >= searchTime)
            {
                currentState = NPCState.Patrol;
                animator.SetBool("walking", true);
                animator.SetBool("lookAround", false);
                GoToNextPatrolPoint();
                PlayVoice(lostFinallyClips);
            }
        }
    }

    // ---------- DETECTION ----------
    void DetectPlayer()
    {
        playerInSight = false;

        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < viewDistance)
        {
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            if (angle < viewAngle / 2)
            {
                if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToPlayer, distance, obstacleMask))
                {
                    if (currentState != NPCState.Attack)
                        PlayVoice(spottedClips);

                    playerInSight = true;
                    if (currentState != NPCState.Attack)
                    {
                        currentState = NPCState.Chase;
                        animator.SetBool("lookAround", false);
                        animator.SetBool("slow running", true);
                    }
                        
                        
                }
            }
        }
    }

    // ---------- HEARING ----------
    public void OnNoiseHeard(Vector3 soundPos)
    {
        if (Vector3.Distance(transform.position, soundPos) <= hearingRadius)
        {
            if (currentState != NPCState.Chase && currentState != NPCState.Attack)
            {
                lastHeardPosition = soundPos;
                currentState = NPCState.Search;
                agent.destination = soundPos;
                stateTimer = 0;
                PlayVoice(heardClips);
                animator.SetBool("lookAround", false);
                animator.SetBool("walking", true);
            }
        }
    }

    // ---------- AUDIO ----------
    void PlayVoice(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.time - lastVoiceTime < minVoiceDelay) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        audioSource.PlayOneShot(clip);
        lastVoiceTime = Time.time;
    }

    // ---------- DEBUG ----------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);

        Gizmos.color = Color.red;
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, left * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
