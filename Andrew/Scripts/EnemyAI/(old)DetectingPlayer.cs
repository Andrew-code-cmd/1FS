using UnityEngine;
using System.Collections;

public class RayScan : MonoBehaviour
{
    [Header("Настройка зрения")]
    // дальность зрения
    public int distance = 15;
    // Угол обзора
    public float angle = 75f;
    // количество лучей 
    public int rays = 12;
    public Vector3 offset;
    // Замечен ли игрок
    public bool playerDetected;
    public Transform target;

    public static Vector3 lastKnownPlayerPosition;

    bool GetRaycast(Vector3 dir)
    {
        bool result = false;
        RaycastHit hit = new RaycastHit();
        Vector3 pos = transform.position + offset;
        if (Physics.Raycast(pos, dir, out hit, distance))
        {
            if (hit.transform == target)
            {
                result = true;
                Debug.DrawLine(pos, hit.point, Color.green);
            }
            else
            {
                Debug.DrawLine(pos, hit.point, Color.blue);
            }
        }
        else
        {
            Debug.DrawRay(pos, dir * distance, Color.red);
        }
        return result;
    }

    bool RayToScan()
    {
        bool result = false;
        bool a = false;
        bool b = false;
        float j = 0;
        for (int i = 0; i < rays; i++)
        {
            var x = Mathf.Sin(j);
            var y = Mathf.Cos(j);

            j += angle * Mathf.Deg2Rad / rays;

            Vector3 dir = transform.TransformDirection(new Vector3(x, 0, y));
            if (GetRaycast(dir)) a = true;

            if (x != 0)
            {
                dir = transform.TransformDirection(new Vector3(-x, 0, y));
                if (GetRaycast(dir)) b = true;
            }
        }

        if (a || b) result = true;
        return result;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, target.position) < distance)
        {
            if (RayToScan())
            {
                lastKnownPlayerPosition = target.transform.position;
                //Debug.Log(lastKnownPlayerPosition);
            }
            else
            {
                // Debug.Log("can't see player");
            }
        }
    }
}