using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public BaseState activeState;
    // свойство для состояния патрулирования
    public PatrolState patrolState;

    public void Initialize()
    {
        // настройка состояния по умолчанию
        patrolState = new PatrolState();
        ChangeState(patrolState);
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        if(activeState != null)
        {
            // вызываем активное состояние
            activeState.Perform();
        }
    }
    public void ChangeState(BaseState newState)
    {
        // проверяем текущее состояние 
        if (activeState != null)
            // выполняем очистку состояния
            activeState.Exit();
        // переходим в новое состояние
        activeState = newState;

        // проверяем новое состояние на null
        if(activeState != null)
        {
            // устанавливаем новое состояние
            activeState.stateMachine = this;
            activeState.enemy = GetComponent<Enemy>();
            activeState.Enter();
        }
    }
}
