using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class DoorLookController : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorTransform;     // Объект, который поворачиваем (сама дверь или родитель)
    public float openAngle = 90f;       // Угол открытия
    public float openSpeed = 4f;        // Скорость открытия/закрытия

    [Header("Interaction Settings")]
    public float interactDistance = 2.5f; // Максимальная дистанция, с которой можно открыть
    public KeyCode interactKey = KeyCode.E; // Клавиша взаимодействия
    public string playerTag = "Player";    // Тег игрока (для безопасности)

    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Coroutine rotating;
    private Camera playerCamera;

    [Header("UI Prompt")]
    public CanvasGroup hintCanvasGroup;
    public Text hintText;     // UI-элемент "Нажмите [E]"
    public float fadeSpeed = 4f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip closeSound;

    void Start()
    {
        if (hintCanvasGroup != null)
            hintCanvasGroup.alpha = 0f;

        if (doorTransform == null)
            doorTransform = transform;

        closedRot = doorTransform.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);

        // Находим камеру игрока
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            playerCamera = player.GetComponentInChildren<Camera>();
    }

    void Update()
    {
        bool lookingAtBtn = false;

        if (playerCamera == null) return;

        // Луч из центра камеры вперёд
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Проверяем, что луч попал именно в эту дверь
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                lookingAtBtn = true;
                // Игрок смотрит на дверь достаточно близко
                if (Input.GetKeyDown(interactKey))
                {
                    ToggleDoor();
                }
            }
        }

        // Плавное появление подсказки
        float targetAlpha = lookingAtBtn ? 1f : 0f;
        if (hintCanvasGroup != null)
            hintCanvasGroup.alpha = Mathf.MoveTowards(hintCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        // Обновление текста и взаимодействие
        if (lookingAtBtn && hintText != null)
            hintText.text = $"Нажмите {interactKey} чтобы открыть дверь";
    }

    void ToggleDoor()
    {
        if (!isOpen)
            audioSource.PlayOneShot(clickSound);
        else
            audioSource.PlayOneShot(closeSound);
        if (rotating != null) StopCoroutine(rotating);
        rotating = StartCoroutine(RotateDoor(isOpen ? closedRot : openRot));
        isOpen = !isOpen;
    }

    IEnumerator RotateDoor(Quaternion target)
    {
        while (Quaternion.Angle(doorTransform.localRotation, target) > 0.1f)
        {
            doorTransform.localRotation = Quaternion.Slerp(
                doorTransform.localRotation,
                target,
                Time.deltaTime * openSpeed
            );
            yield return null;
        }

        doorTransform.localRotation = target;
        rotating = null;
    }
}
