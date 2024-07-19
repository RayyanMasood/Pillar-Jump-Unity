using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot : MonoBehaviour
{
    public Rigidbody2D rb;
    public LineRenderer lineRenderer;
    public GameObject dotPrefab;
    public int numberOfDots = 20;
    public float maxLaunchForce = 10f;
    public float lineWidth = 0.1f;
    public float dotSpacing = 0.1f;
    public float dotTransparency = 0.5f;
    public float minimalMovementThreshold = 0.001f; // Minimal movement threshold to reset state
    public float stickingMargin = 5f; // Margin of error for sticking to the top collider

    // UI elements
    public TextMeshProUGUI respawnCounterText;
    public Image aimIndicator;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragging = false;
    private bool isLaunched = false;
    private bool isLaunchable = false;
    private bool isInitialDropComplete = false;
    private List<GameObject> trajectoryDots = new List<GameObject>();

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Camera mainCamera;
    private int respawnCount = 0;

    void Start()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        CreateDots();

        initialPosition = transform.position;
        initialRotation = transform.rotation;
        mainCamera = Camera.main;

        // Allow the player to drop onto the platform initially
        rb.constraints = RigidbodyConstraints2D.None;

        // Initialize UI elements
        UpdateRespawnCounter();
        aimIndicator.color = Color.red;
    }

    void Update()
    {
        if (!isInitialDropComplete)
        {
            // Check if the player has landed on a platform
            if (isLaunchable)
            {
                isInitialDropComplete = true;
                rb.constraints = RigidbodyConstraints2D.FreezeAll; // Freeze after initial drop
            }
            return; // Skip the rest of the update loop until the initial drop is complete
        }

        if (isLaunchable && Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            lineRenderer.positionCount = 2; // Ensure the line renderer has 2 positions
            isDragging = true;

            // Detach from any parent platform
            transform.SetParent(null);
        }

        if (isDragging)
        {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            DrawLine(startPos, endPos);
            DisplayTrajectory(startPos, endPos);
            UpdateAimIndicator(); // Add this to update the aim indicator
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (Vector2.Distance(startPos, endPos) < minimalMovementThreshold)
            {
                // If the drag distance is too short, reset dragging state
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                isLaunchable = true; // Allow launching again
            }
            else
            {
                isLaunched = true;
                isLaunchable = false; // Prevent launching again
                rb.constraints = RigidbodyConstraints2D.None;
                ShootPlayer(startPos, endPos);
            }
            lineRenderer.positionCount = 0;  // Hide the line
            HideDots();
        }

        if (!IsInCameraBounds())
        {
            Respawn();
        }

        // Check if player is stuck with minimal movement after launch
        if (isLaunched && rb.velocity.magnitude < minimalMovementThreshold)
        {
            ResetLaunch();
        }
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    void ShootPlayer(Vector2 start, Vector2 end)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance, 0, maxLaunchForce);
        rb.velocity = force;

        // Reset rotation to initial rotation
        transform.rotation = initialRotation;

        isLaunchable = false;
    }

    void CreateDots()
    {
        for (int i = 0; i < numberOfDots; i++)
        {
            GameObject dot = Instantiate(dotPrefab);
            dot.SetActive(false);
            Color dotColor = dot.GetComponent<SpriteRenderer>().color;
            dotColor.a = dotTransparency;
            dot.GetComponent<SpriteRenderer>().color = dotColor;
            trajectoryDots.Add(dot);
        }
    }

    void DisplayTrajectory(Vector2 start, Vector2 end)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance, 0, maxLaunchForce);

        Vector2 pos = (Vector2)transform.position;
        Vector2 velocity = force;

        for (int i = 0; i < numberOfDots; i++)
        {
            float t = i * dotSpacing;
            Vector2 dotPos = pos + velocity * t + 0.5f * Physics2D.gravity * (t * t);
            trajectoryDots[i].SetActive(true);
            trajectoryDots[i].transform.position = dotPos;
        }
    }

    void HideDots()
    {
        foreach (var dot in trajectoryDots)
        {
            dot.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Top Collider"))
        {
            isLaunchable = true;

            if (isLaunched)
            {
                isLaunched = false;
                rb.velocity = Vector2.zero;

                // Check if the player is aligned with the top collider within the margin of error
                float angle = Mathf.Abs(transform.eulerAngles.z % 90);
                if (angle < stickingMargin || angle > (90 - stickingMargin))
                {
                    rb.constraints = RigidbodyConstraints2D.FreezeAll; // Freeze position and rotation
                }
                else
                {
                    rb.constraints = RigidbodyConstraints2D.None; // Allow the player to adjust position
                }
            }
        }
    }

    

    bool IsInCameraBounds()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.x >= -0.05 && viewportPosition.x <= 1.05 && viewportPosition.y >= -0.05 && viewportPosition.y <= 1.05;
    }

    void Respawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f; // Reset angular velocity
        rb.constraints = RigidbodyConstraints2D.None; // Allow physics to apply after respawn
        isLaunched = false;
        isLaunchable = false; // Prevent launching while dropping
        isInitialDropComplete = false; // Reset initial drop flag
        respawnCount++; // Increment respawn count
        UpdateRespawnCounter(); // Update the UI counter
    }

    void ResetLaunch()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        isLaunched = false;
        isLaunchable = true; // Allow launching again
    }

    void UpdateRespawnCounter()
    {
        respawnCounterText.text = respawnCount.ToString();
    }

    void UpdateAimIndicator()
    {
        float closestDistance = float.MaxValue;
        Vector2 arcEndPosition = trajectoryDots[numberOfDots - 1].transform.position;

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(arcEndPosition, 10f))
        {
            if (collider.CompareTag("Top Collider"))
            {
                float distance = Vector2.Distance(arcEndPosition, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }

        // Smoothly transition the color based on the closest distance
        float maxDistance = 5f; // Maximum distance for the color transition
        float t = Mathf.Clamp01(closestDistance / maxDistance);
        aimIndicator.color = Color.Lerp(Color.green, Color.red, t);
    }
}
