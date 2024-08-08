using System.Collections.Generic;
using UnityEngine;

public class TrajectoryManager : MonoBehaviour
{
    public static TrajectoryManager Instance { get; private set; }
    public GameObject dotPrefab;
    public bool enableDottedTrajectory = false; // Toggle for enabling/disabling the dotted trajectory
    public bool enableLandingDot = false; // Toggle for enabling/disabling the landing dot
    public int numberOfDots = 20;
    public float dotSpacing = 0.1f;
    public float dotTransparency = 0.5f;

    private List<GameObject> trajectoryDots = new List<GameObject>();
    private GameObject currentDot; // Keep track of the current collision dot

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateDots();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateDots()
    {
        for (int i = 0; i < numberOfDots; i++)
        {
            GameObject dot = Instantiate(dotPrefab);
            dot.SetActive(false);
            dot.tag = "TrajectoryDot"; // Tag the dots for detection
            Color dotColor = dot.GetComponent<SpriteRenderer>().color;
            dotColor.a = dotTransparency;
            dot.GetComponent<SpriteRenderer>().color = dotColor;
            trajectoryDots.Add(dot);
        }
    }

    public void DrawLine(Vector2 start, Vector2 end, LineRenderer lineRenderer, float lineWidth, Color lineColor)
    {
        lineRenderer.positionCount = 2; // Ensure the line renderer has 2 positions
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.material.color = lineColor; // Ensure the material color is also set
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public Vector2 DisplayTrajectory(Vector2 start, Vector2 end, Vector2 initialPosition, Rigidbody2D rb, float maxLaunchForce, float launchSensitivity)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance * launchSensitivity, 0, maxLaunchForce);

        Vector2 pos = initialPosition;
        Vector2 velocity = force;
        Vector2 gravity = Physics2D.gravity * rb.gravityScale;

        float timeStep = 0.02f; // smaller time steps for more accuracy

        if (enableDottedTrajectory)
        {
            for (int i = 0; i < numberOfDots; i++)
            {
                float t = i * dotSpacing;
                Vector2 dotPos = pos + velocity * t + 0.5f * gravity * (t * t);
                trajectoryDots[i].SetActive(true);
                trajectoryDots[i].transform.position = dotPos;
            }
        }
        else
        {
            HideDots();
        }

        for (float t = 0; t < 5f; t += timeStep) // Simulate for 5 seconds with small time steps
        {
            RaycastHit2D hit = Physics2D.Raycast(pos, velocity, velocity.magnitude * timeStep, ~LayerMask.GetMask("Player"));
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                Debug.Log("Raycast hit: " + hit.collider.name + " at position: " + hit.point);
                if (enableLandingDot)
                {
                    CreateDot(hit.point); // Create a dot at the collision point if enabled
                }
                return hit.point;
            }

            pos += velocity * timeStep;
            velocity += gravity * timeStep;
        }

        Debug.Log("Raycast did not hit any collider.");
        DestroyDot(); // Hide the dot if no collision
        return pos;
    }

    private void CreateDot(Vector2 position)
    {
        // Destroy the previous dot if it exists
        if (currentDot != null)
        {
            Destroy(currentDot);
        }

        // Create and store the new dot
        if (dotPrefab != null)
        {
            currentDot = Instantiate(dotPrefab, position, Quaternion.identity);
            currentDot.SetActive(true);
        }
        else
        {
            Debug.LogError("Dot Prefab is not assigned in the TrajectoryManager.");
        }
    }

    private void DestroyDot()
    {
        if (currentDot != null)
        {
            Destroy(currentDot);
            currentDot = null;
        }
    }

    public void HideDots()
    {
        foreach (var dot in trajectoryDots)
        {
            dot.SetActive(false);
        }
        DestroyDot();
    }

    public void FlipDots(bool flip)
    {
        foreach (var dot in trajectoryDots)
        {
            if (flip)
            {
                dot.transform.localScale = new Vector3(dot.transform.localScale.x, -Mathf.Abs(dot.transform.localScale.y), dot.transform.localScale.z);
            }
            else
            {
                dot.transform.localScale = new Vector3(dot.transform.localScale.x, Mathf.Abs(dot.transform.localScale.y), dot.transform.localScale.z);
            }
        }
    }
}
