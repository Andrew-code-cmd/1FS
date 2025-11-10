using Unity.VisualScripting;
using UnityEngine;

public class PlayerLean : MonoBehaviour
{
    public Animator cameraAnim;
    public LayerMask layers;
    RaycastHit hit;

    void Update()
    {
        if (Input.GetKey(KeyCode.Q) && !Physics.Raycast(transform.position, transform.right, out hit, 1f, layers))
        {
            cameraAnim.ResetTrigger("idle");
            cameraAnim.ResetTrigger("right");
            cameraAnim.SetTrigger("left");
        }
        else if (Input.GetKey(KeyCode.E) && !Physics.Raycast(transform.position, -transform.right, out hit, 1f, layers))
        {
            cameraAnim.ResetTrigger("idle");
            cameraAnim.ResetTrigger("left");
            cameraAnim.SetTrigger("right");
        }
        else
        {
            cameraAnim.ResetTrigger("right");
            cameraAnim.ResetTrigger("left");
            cameraAnim.SetTrigger("idle");
        }
    } 
}
