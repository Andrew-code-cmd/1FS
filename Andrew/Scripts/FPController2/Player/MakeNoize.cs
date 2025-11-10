using UnityEngine;

public class NoiseSource : MonoBehaviour
{
    public static void MakeNoise(Vector3 position, float radius)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out EnemyHearing hearing))
                hearing.HearNoise(position);
        }
    }
}

