using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    [Header("Настройка скорости движения и гравитации")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Сенса")]
    public float mouseSensitivity = 2f;

    [Header("Слой земли")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("Настройка камеры (угол обзора при ходьбе и беге)")]
    public float normalFOV = 60f;       // ����������� ���� ������
    public float runFOV = 70f;          // ���� ������ ��� ����
    public float fovChangeSpeed = 6f;   // �������� ��������

    [Header("Стамина")]
    public float maxStamina = 100f;           // ������������ ����� ������������
    public float currentStamina;              // ������� ������������
    public float staminaDrainRate = 20f;      // �������� ������� ��� ����
    public float staminaRegenRate = 10f;      // �������� ��������������
    public float regenDelay = 1.5f;           // �������� ����� ��������������� ����� ����
    public Slider staminaBar;

    [Header("Настройки приседания")]
    public float crouchHeight = 1.0f;
    public float standHeight = 2.0f;
    public float crouchSpeed = 0.2f;
    public float cameraCrouchOffset = -0.5f;

    public bool isCrouching = false;
    private Vector3 cameraDefaultLocalPos;

    [Header("Аудио")]
    public AudioSource audioSource;

    public AudioClip[] jumpSounds;
    public AudioClip breathSoundSlip;
    public AudioClip takeTheSitSound;


    public bool isRunning = false;
    public bool isGrounded;
    public bool isMoving;
    private bool isBreathing;


    private float regenTimer = 0f;
    CharacterController controller;
    Camera playerCamera;
    public Transform playerCameraTransform;
    public Vector3 velocity;
    float xRotation = 0f;
    float targetFOV;
    private float lastVoiceTime;
    public float minVoiceDelay = 1f;

    public float currentSpeed;
    bool canRun = true;

    void Start()
    {
        currentStamina = maxStamina;
        if (staminaBar != null)
            staminaBar.maxValue = maxStamina;

        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("PlayerController: Camera not found as child! Make sure the main Camera is inside Player.");
            enabled = false;
            return;
        }
        cameraDefaultLocalPos = playerCameraTransform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;

        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = check.transform;
        }

        targetFOV = normalFOV;
        playerCamera.fieldOfView = normalFOV;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleFOV();

        if (Input.GetKeyDown(KeyCode.LeftControl))
            ToggleCrouch();
    }

    void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        canRun = isCrouching ? false : true;
        StartCoroutine(ChangeHeight(isCrouching));
    }

    IEnumerator ChangeHeight(bool crouch)
    {
        audioSource.PlayOneShot(takeTheSitSound);
        float targetHeight = crouch ? crouchHeight : standHeight;
        walkSpeed = crouch ? 2f : 5f;
        Vector3 targetCameraPos = cameraDefaultLocalPos + Vector3.up * (crouch ? cameraCrouchOffset : 0f);
        float t = 0f;

        float startHeight = controller.height;
        Vector3 startCam = playerCameraTransform.localPosition;

        while (t < 1f)
        {
            t += Time.deltaTime * (1f / crouchSpeed);
            controller.height = Mathf.Lerp(startHeight, targetHeight, t);
            playerCameraTransform.localPosition = Vector3.Lerp(startCam, targetCameraPos, t);
            yield return null;
        }

        controller.height = targetHeight;
        playerCameraTransform.localPosition = targetCameraPos;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        isMoving = Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0f && canRun;
        currentSpeed = isRunning ? runSpeed : walkSpeed;


        // ������ ������� ��� ���� 
        if (isRunning)
        {
            // ������ �������
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            if (currentStamina == 0f)
                if(!isBreathing)
                    StartCoroutine(breathSound());
            regenTimer = 0f;
        }
        else
        {
            // �������������� ����� ��������
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            }
        }
        if (staminaBar != null)
            staminaBar.value = currentStamina;


        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            PlayVoice(jumpSounds);
        }
            

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // FOV ��� ����
        targetFOV = isRunning && (x != 0 || z != 0) ? runFOV : normalFOV;
    }

    private IEnumerator breathSound()
    {
        audioSource.PlayOneShot(breathSoundSlip);
        isBreathing = true;
        yield return new WaitForSeconds(7);
        isBreathing = false;
    }

    void HandleFOV()
    {
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            Time.deltaTime * fovChangeSpeed
        );
    }

    void PlayVoice(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        if (Time.time - lastVoiceTime < minVoiceDelay) return; // защита от спама

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);

        lastVoiceTime = Time.time;
    }
}
