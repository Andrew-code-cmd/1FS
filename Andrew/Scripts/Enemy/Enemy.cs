using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    private StateMachine stateMachine;
    private NavMeshAgent agent; // вот это оригинальный Агент
    public NavMeshAgent Agent { get => agent; } // а это свойство, позволяющее получить ориг. агент из других скриптов 
    [SerializeField] private string currentState;
    public EnemyPath path;

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stateMachine.Initialize();
    }

    void Update()
    {

    }
}
