using UnityEngine;

public class TVSoundOcclusion : MonoBehaviour
{
    public Transform player;
    public AudioLowPassFilter lowPassFilter;
    public float maxDistance = 15f;
    public LayerMask obstructionMask;
    public float cutoffBehindWall = 800f;
    public float cutoffClear = 5000f;

    void Update()
    {
        if (!player || !lowPassFilter) return;

        Vector3 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // ѕровер€ем, есть ли преп€тствие между телевизором и игроком
        bool blocked = Physics.Raycast(transform.position, direction, distance, obstructionMask);

        // ≈сли далеко или за стеной Ч приглушаем
        if (blocked || distance > maxDistance)
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, cutoffBehindWall, Time.deltaTime * 2);
        else
            lowPassFilter.cutoffFrequency = Mathf.Lerp(lowPassFilter.cutoffFrequency, cutoffClear, Time.deltaTime * 2);
    }
}
