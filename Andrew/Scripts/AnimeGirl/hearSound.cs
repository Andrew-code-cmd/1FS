using UnityEngine;
using UnityEngine.AI;

public class EnemyHearing : MonoBehaviour
{
    [Header("Настройки реакции на звук")]
    public AudioSource audioSource;             // Источник звука на NPC
    public AudioClip[] heardClip;                 // Реплика при обнаружении шума
    public AudioClip[] spottedClip;
    public float investigateTime = 3f;          // Время "осмотра" точки шума
    public float moveSpeed = 3.5f;              // Скорость движения NPC

    [Header("Компоненты")]
    private NavMeshAgent agent;
    private Vector3 lastHeardPosition;
    private bool isInvestigating = false;
    private Animator animator;

    [Header("Другое")]
    public NPCFollow npcScript;
    private float soundTimer;
    public float spottedSoundInterval = 5f; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = moveSpeed;
        animator = GetComponent<Animator>();
    }

    public void HearNoise(Vector3 position)
    {
        if (isInvestigating) return; // Чтобы не перебивать текущую реакцию

        lastHeardPosition = position;
        isInvestigating = true;

        // Проиграть реплику (если есть)
        if (audioSource != null && heardClip != null && !npcScript.playerDetected)
        {
            audioSource.clip = heardClip[Random.Range(0, heardClip.Length)];
            audioSource.Play();
        }

        animator.SetBool("Moving", true);
        animator.SetBool("Walking", true);
        agent.SetDestination(position);

        // Вернуться к обычному состоянию после проверки
        Invoke(nameof(StopInvestigating), investigateTime);
    }

    public void PlaySpottedSound()
    {
        soundTimer -= Time.deltaTime;
        if (soundTimer <= 0f)
        {
            if (audioSource && spottedClip[0])
                audioSource.PlayOneShot(spottedClip[Random.Range(0, spottedClip.Length)]);
            soundTimer = spottedSoundInterval;
        }
    }

    private void StopInvestigating()
    {
        isInvestigating = false;
        // Здесь можно добавить возврат к патрулю, если он есть
    }
}
