using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public Camera cam;                // Ссылка на камеру
    public float zoomFOV = 40f;       // Поле зрения при зуме
    public float normalFOV = 60f;     // Обычное поле зрения
    public float zoomSpeed = 10f;     // Скорость перехода

    private bool isZoomed = false;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // ПКМ нажата
            isZoomed = true;

        if (Input.GetMouseButtonUp(1))   // ПКМ отпущена
            isZoomed = false;

        float targetFOV = isZoomed ? zoomFOV : normalFOV;

        // Плавный переход между зумом и нормальным видом
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }
}
