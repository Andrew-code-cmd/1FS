using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    public float noiseRadius = 5f;
    //public LayerMask hearingMask;
    public PlayerControls playerControls;

    public void MakeNoise()
    {
        Collider[] heardBy = Physics.OverlapSphere(transform.position, noiseRadius);
        foreach (Collider c in heardBy)
        {
            c.SendMessage("OnNoiseHeard", transform.position, SendMessageOptions.DontRequireReceiver);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || playerControls.isRunning)
            MakeNoise();
    }
}
