using UnityEngine;
using UnityEngine.AI;

public class NPCFollow : MonoBehaviour
{
    public Transform player;             // ������ �� ������ ������
    public float visionRange = 10f;      // ������, � ������� NPC "�����" ������
    public float stopDistance = 1f;      // �� ����� ���������� NPC ���������������
    public float viewAngle = 60f;        // ���� ������ NPC
    private NavMeshAgent agent;
    public bool playerDetected;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null) return;

        if (!playerDetected)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (distanceToPlayer <= visionRange)
            {
                float angle = Vector3.Angle(transform.forward, directionToPlayer);

                if (angle <= viewAngle / 2f)
                {
                    if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, visionRange))
                    {
                        if (hit.transform == player)
                        {
                            playerDetected = true;
                        }
                    }
                }
            }
        }

        if (playerDetected)
        {

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > stopDistance)
            {

                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            else
            {

                agent.isStopped = true;
            }
        }
    }
}
