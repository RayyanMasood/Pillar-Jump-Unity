using UnityEngine;
using System.Collections.Generic;

public class GravityZoneScript : MonoBehaviour
{
    private Dictionary<Rigidbody2D, float> originalGravityScales = new Dictionary<Rigidbody2D, float>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D objectRigidbody = other.GetComponent<Rigidbody2D>();
        if (objectRigidbody != null && !originalGravityScales.ContainsKey(objectRigidbody))
        {
            Debug.Log("Enter: " + other.name);
            originalGravityScales[objectRigidbody] = objectRigidbody.gravityScale;
            objectRigidbody.gravityScale = -Mathf.Abs(objectRigidbody.gravityScale); // Reverse gravity
        }

        // Check if the object is a trajectory dot
        if (other.CompareTag("TrajectoryDot"))
        {
            Debug.Log("TD Enter");
            TrajectoryManager.Instance.FlipDots(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Rigidbody2D objectRigidbody = other.GetComponent<Rigidbody2D>();
        if (objectRigidbody != null && originalGravityScales.ContainsKey(objectRigidbody))
        {
            Debug.Log("Exit: " + other.name);
            objectRigidbody.gravityScale = originalGravityScales[objectRigidbody]; // Restore original gravity
            originalGravityScales.Remove(objectRigidbody);
        }

        // Check if the object is a trajectory dot
        if (other.CompareTag("TrajectoryDot"))
        {
            TrajectoryManager.Instance.FlipDots(false);
        }
    }
}
