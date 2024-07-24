using UnityEngine;

public class PlayerTriggerHandler : MonoBehaviour
{
    public GrapplingGun grapplingGun; // Reference to the GrapplingGun script
    private Rigidbody2D playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HookInfluence"))
        {
            Debug.Log("Player entered HookInfluence trigger");
            grapplingGun.SetGrapplingGunActive(true);
            grapplingGun.SetHookPoint(other.transform.parent);
            FreezePlayerRotation(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("HookInfluence"))
        {
            Debug.Log("Player exited HookInfluence trigger");
            grapplingGun.SetGrapplingGunActive(false);
            grapplingGun.ClearHookPoint();
            grapplingGun.ReleaseGrapple();
            FreezePlayerRotation(false);
        }
    }

    private void FreezePlayerRotation(bool freeze)
    {
        if (freeze)
        {
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            playerRigidbody.constraints = RigidbodyConstraints2D.None;
        }
    }
}
