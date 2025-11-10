using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SlipperyCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float deceleration = 1.5f;           // скорость замедления на льду
    public float normalDeceleration = 10f;      // скорость остановки на обычной поверхности
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private Vector3 currentMomentum;
    private bool isGrounded;
    private bool onIce;

    public bool enableWetFloor;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {

        if (enableWetFloor)
        {
            Debug.Log("enabled wet floor");
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 inputDir = transform.right * x + transform.forward * z;

            if (inputDir.magnitude > 0.1f)
            {
                moveDirection = inputDir.normalized * moveSpeed;
                currentMomentum = moveDirection; // обновляем импульс, если есть ввод
            }
            else if (isGrounded)
            {
                // выбираем нужное замедление в зависимости от поверхности
                float currentDecel = onIce ? deceleration : normalDeceleration;
                currentMomentum = Vector3.Lerp(currentMomentum, Vector3.zero, Time.deltaTime * currentDecel);
            }

            // Гравитация
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;

            // Двигаем контроллер
            controller.Move((currentMomentum + velocity) * Time.deltaTime);
        }
    }
}
