using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;
    private bool isOn = false;
    public AudioSource audioSource;
    public AudioClip flashLightClickSound;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isOn = !isOn;
            flashlight.enabled = isOn;
            audioSource.PlayOneShot(flashLightClickSound);
        }
    }
}
