using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Footsteps : MonoBehaviour
{
    [Header("Footstep Settings")]
    public AudioSource audioSource;        // �������� ����� (�������� �� ������)
    public AudioClip[] woodFootsteps;     // ������ ����� �����
    public AudioClip[] metalFootsteps;
    public AudioClip[] grassFootSteps;

    public float stepInterval = 0.5f;      // ����� ����� ������ (��� ������� ������ � ������)
    public float runStepInterval = 0.33f;
    public float walkSpeedThreshold = 0.1f; // ����������� �������� ��� ����� �����

    private CharacterController controller;
    private float stepTimer;
    private PlayerControls plCtrl;

    bool wasGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        plCtrl = GetComponent<PlayerControls>();
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;
        bool isMoving = (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f
                      || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f);
        bool isRunning = plCtrl.isRunning;
        

        // ����� ��� ������
        if (isGrounded && !isRunning && !plCtrl.isCrouching && isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else if(isGrounded && isRunning && isMoving && !plCtrl.isCrouching)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = runStepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        AudioClip clip = GetFootstepSound();
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    // ПРОВЕРЯЕМ ПОВЕРХНОСТЬ ПОД ИГРОКОМ (МЕТАЛ, ДЕРЕВО, ТРАВА). ПОДСТАВЛЯЕМ ЗВУКИ ШАГОВ В ЗАВИСИМОСТИ ОТ ПОВЕРХНОСТИ 
    AudioClip GetFootstepSound()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            switch (hit.collider.tag)
            {
                case "Ground":
                    return woodFootsteps[Random.Range(0, woodFootsteps.Length-1)];
                case "Metal":
                    return metalFootsteps[Random.Range(0, metalFootsteps.Length-1)];
                case "Grass":
                    return grassFootSteps[Random.Range(0, metalFootsteps.Length-1)];
            }
        }
        // Debug.Log("not recognized");
        // если поверхность не опознана — шаг по умолчанию
        return woodFootsteps[Random.Range(0, woodFootsteps.Length)];
    }

}
