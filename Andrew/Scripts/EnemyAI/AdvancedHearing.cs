using UnityEngine;

public class AdvancedHearing : MonoBehaviour
{
    public float maxHearingDistance = 15f;
    public LayerMask obstacleMask;

    public static Vector3 noiseSoundPosition;

    public bool CanHear(Vector3 soundPosition)
    {
        float distance = Vector3.Distance(transform.position, soundPosition);
        if (distance > maxHearingDistance)
            return false;

        // Проверяем, не мешают ли препятствия
        if (Physics.Raycast(soundPosition, (transform.position - soundPosition).normalized, out RaycastHit hit, distance, obstacleMask))
        {
            return false; // что-то перекрыло звук
        }

        return true;
    }

    public void OnNoiseHeard(Vector3 soundPosition)
    {
        if (CanHear(soundPosition))
        {
            //Debug.Log($"{name} услышал шум на расстоянии {Vector3.Distance(transform.position, soundPosition)}");
            noiseSoundPosition = soundPosition;
        }
    }
}
