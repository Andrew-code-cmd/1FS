using UnityEngine;

public class slipperyZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
    }
}
