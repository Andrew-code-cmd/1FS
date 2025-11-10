using Unity.VisualScripting;
using UnityEngine;

public class toggleWetFloorMode : MonoBehaviour
{
    SlipperyCharacterController wetFloorScript;

    private void Start()
    {
        wetFloorScript = GetComponent<SlipperyCharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("player entered");
            wetFloorScript.enableWetFloor = true;
            Debug.Log(wetFloorScript.enableWetFloor);
        }
    }
}
