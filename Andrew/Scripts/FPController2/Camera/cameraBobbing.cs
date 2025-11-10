using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem.XR;

public class CameraBobbing : MonoBehaviour
{
    [Header("ѕараметры покачивани€")]
    public PlayerControls player; 
    public float walkamplitude = 0f; // высота покачивани€
    public float runamplitude = 0.40f;
    public float walkfrequency = 12.8f;  // частота покачивани€
    public float runFrequensy = 15f;
    public float smooth = 3f;    // сглаживание при остановке

    private Vector3 startPos;
    private Quaternion startRot;
    private float bobTimer;
    //private float footstepTimer;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        if (player == null) return;

        float offsetY, offsetX;

        if (player.isMoving)
        {
            if (!player.isRunning)
            {
                bobTimer += Time.deltaTime * walkfrequency;
                offsetY = Mathf.Sin(bobTimer) * walkamplitude;
                offsetX = Mathf.Cos(bobTimer * 0.5f) * walkamplitude * 0.5f;
            }
            else
            {
                bobTimer += Time.deltaTime * runFrequensy;
                offsetY = Mathf.Sin(bobTimer) * runamplitude;
                offsetX = Mathf.Cos(bobTimer * 0.5f) * runamplitude * 0.5f;
            }

            Vector3 targetPos = startPos + new Vector3(offsetX, offsetY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smooth);
        }
        else
        {
            // ¬озврат к исходному положению
            bobTimer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * smooth);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, startRot, Time.deltaTime * smooth);
            //footstepTimer = 0f;
        }
    }
}
