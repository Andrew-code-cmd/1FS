using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    public enum NPCState { Patrol, Chase, Attack, Search, Stealth, Slip }

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;
    public Transform[] stealthPoints; // точки укрытия
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;

    [Header("Vision Settings")]
    public float viewDistance = 12f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("Hearing Settings")]
    public float hearingRadius = 10f;

    [Header("Behaviour Settings")]
    public float searchTime = 5f;
    public float lostSightTime = 3f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float waitAfterAttack = 2f;
    public float stealthWaitTime = 10f;
    public float stealthTriggerDistance = 6f;
    public float attackDamage = 10f;

    [Header("Slippery Settings")]
    public float slipRecoveryTime = 3f;
    public AudioClip[] slipClips;


    [Header("Voice Lines")]
    public AudioClip[] heardClips;
    public AudioClip[] spottedClips;
    public AudioClip[] lostClips;
    public AudioClip[] attackClips;
    public AudioClip[] stealthClips;
    public AudioClip[] lostFinallyClips;

    public float minVoiceDelay = 5f;

    private NPCState currentState;
    private int currentPatrolIndex;
    private float stateTimer;
    private float lastVoiceTime;
    private float lastAttackTime;
    private float forWaitAfterAttack;
    private Vector3 lastHeardPosition;
    private bool playerInSight;
    private bool hiding;

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
            case NPCState.Patrol: Patrol(); break;
            case NPCState.Chase: Chase(); break;
            case NPCState.Attack: Attack(); break;
            case NPCState.Search: Search(); break;
            case NPCState.Stealth: Stealth(); break;
            case NPCState.Slip: Slip(); break;
        }

        DetectPlayer();
    }

    // ---------- PATROL ----------
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();

        // шанс перейти в режим Stealth случайно или после потери игрока
        //if (Random.value < 0.001f && stealthPoints.Length > 0)
        //{
        //    GoToStealthPoint();
        //}
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
                // NPC решает спрятаться
                GoToStealthPoint();
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

        agent.speed = 0f;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance > attackRange * 1.3f)
        {
            forWaitAfterAttack = Time.time;

            if (forWaitAfterAttack >= 3f)
            {
                currentState = NPCState.Chase;
                agent.isStopped = false;
                agent.speed = 2.5f;
                animator.SetBool("battle", false);
                animator.SetBool("slow running", true);
                animator.SetBool("walking", false);

                return;
            }
        }

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            PlayVoice(attackClips);
            animator.SetBool("battle", true);
            animator.SetBool("slow running", false);
            animator.SetBool("walking", false);
        }
    }

    public void OnAttackHit()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attackRange + 0.5f)
        {
            Debug.Log($"{name} ударил игрока на {attackDamage} урона");
        }
    }

    // ---------- SEARCH ----------
    void Search()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            animator.SetBool("walking", false);
            animator.SetBool("slow running", false);
            animator.SetBool("walking", false);
            animator.SetBool("battle", false);
            animator.SetBool("lookAround", true);

            stateTimer += Time.deltaTime;
            if (stateTimer >= searchTime)
            {
                GoToStealthPoint();
                animator.SetBool("walking", true);
                animator.SetBool("lookAround", false);
                PlayVoice(lostFinallyClips);
            }
        }
    }

    // ---------- STEALTH ----------
    void GoToStealthPoint()
    {
        if (stealthPoints.Length == 0) return;

        Transform bestPoint = null;
        float bestDistance = Mathf.Infinity;

        foreach (Transform point in stealthPoints)
        {
            if (point == null) continue;

            float dist = Vector3.Distance(transform.position, point.position);
            bool visibleToPlayer = false;

            if (player != null)
            {
                Vector3 origin = player.position + Vector3.up * 1.5f;
                Vector3 target = point.position + Vector3.up * 1.5f;
                float distance = Vector3.Distance(origin, target);

                // Если игрок видит это укрытие — оно не подходит
                if (!Physics.Raycast(origin, (target - origin).normalized, distance, obstacleMask))
                    visibleToPlayer = true;
            }

            // Выбираем ближайшее невидимое укрытие
            if (!visibleToPlayer && dist < bestDistance)
            {
                bestPoint = point;
                bestDistance = dist;
            }
        }

        // Если не найдено невидимое — берём ближайшее
        if (bestPoint == null)
        {
            bestPoint = stealthPoints[0];
            bestDistance = Vector3.Distance(transform.position, bestPoint.position);

            foreach (Transform point in stealthPoints)
            {
                if (point == null) continue;
                float dist = Vector3.Distance(transform.position, point.position);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    bestPoint = point;
                }
            }
        }

        if (bestPoint != null)
        {
            agent.isStopped = false;
            agent.destination = bestPoint.position;
            currentState = NPCState.Stealth;
            stateTimer = 0;
            hiding = false;
            PlayVoice(stealthClips);
            Debug.Log($"{name} выбрал укрытие: {bestPoint.name}");

            animator.SetBool("crouch", true);
            animator.SetBool("walking", false);
        }
    }

    void Stealth()
    {

        if (!hiding && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            hiding = true;
            animator.SetBool("walking", false);
            animator.SetBool("standToCover", true);
            animator.SetBool("crouch", false);
            agent.isStopped = true;
            stateTimer = 0;
            Debug.Log($"{name} спрятался в укрытии и выжидает игрока");
        }

        if (hiding)
        {
            stateTimer += Time.deltaTime;

            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, player.position);

            // transform.rotation = Quaternion.LookRotation(dirToPlayer);

            // Игрок приближается — нападение
            if (distance < stealthTriggerDistance &&
                !Physics.Raycast(transform.position + Vector3.up, dirToPlayer, distance, obstacleMask))
            {
                animator.SetBool("walking", false);
                animator.SetBool("standToCover", false);
                animator.SetBool("slow running", true);
                animator.SetBool("crouch", false);
                currentState = NPCState.Chase;
                agent.isStopped = false;
                PlayVoice(spottedClips);
                Debug.Log($"{name} заметил игрока из укрытия и начинает погоню!");
            }
            // Игрок не пришёл — возвращаемся к патрулю
            else if (stateTimer > stealthWaitTime)
            {
                animator.SetBool("walking", true);
                animator.SetBool("standToCover", false);
                animator.SetBool("slow running", false);
                animator.SetBool("crouch", false);
                currentState = NPCState.Patrol;
                agent.isStopped = false;
                GoToNextPatrolPoint();
                Debug.Log($"{name} устал ждать и вернулся к патрулю");
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
                    playerInSight = true;

                    if (currentState == NPCState.Stealth || currentState == NPCState.Patrol || currentState == NPCState.Search)
                    {
                        PlayVoice(spottedClips);
                        animator.SetBool("walking", false);
                        animator.SetBool("standToCover", false);
                        animator.SetBool("slow running", true);
                        animator.SetBool("crouch", false);
                        currentState = NPCState.Chase;
                        agent.isStopped = false;
                    }
                }
            }
        }
    }

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

        bool blocked = false;
        if (player != null)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 target = player.position + Vector3.up * 1.5f;
            float distance = Vector3.Distance(origin, target);
            if (Physics.Raycast(origin, (target - origin).normalized, distance, obstacleMask))
                blocked = true;
        }

        audioSource.volume = blocked ? 0.3f : 1f;
        audioSource.PlayOneShot(clip);
        lastVoiceTime = Time.time;
    }

    void Slip()
    {
        agent.isStopped = true;
        animator.SetBool("walking", false);
        animator.SetBool("slow running", false);
        animator.SetBool("battle", false);
        animator.SetTrigger("Slip"); // триггер для анимации падения

        stateTimer += Time.deltaTime;
        if (stateTimer >= slipRecoveryTime)
        {
            // Вернуться к патрулю или предыдущему состоянию
            agent.isStopped = false;
            animator.ResetTrigger("Slip");
            PlayVoice(lostClips); // можно озвучить "чёрт!" или "ай!"
            currentState = NPCState.Patrol;
            GoToNextPatrolPoint();
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        foreach (var point in stealthPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, 0.5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Slippery") && currentState != NPCState.Slip)
        {
            Debug.Log($"{name} подскользнулся на {other.name}!");
            PlayVoice(slipClips);
            currentState = NPCState.Slip;
            stateTimer = 0;
        }
        Debug.Log(other.tag);
    }
}
