using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public LineRenderer lineRenderer;
    public float maxLaunchForce = 10f;
    public float minimalMovementThreshold = 0.001f;
    public float stickingMargin = 0.1f;
    public float lineWidth = 0.1f;  // Expose line width
    public Color lineColor = Color.white;  // Expose line color

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragging = false;
    private bool isLaunched = false;
    private bool isLaunchable = false;
    private bool isInitialDropComplete = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Camera mainCamera;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        mainCamera = Camera.main;
        rb.constraints = RigidbodyConstraints2D.None; // Allow initial drop

        // Set initial line width
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        // Set the color gradient
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(lineColor, 0.0f), new GradientColorKey(new Color(lineColor.r, lineColor.g, lineColor.b, 0), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;

        // Ensure the material color is set
        lineRenderer.material.color = lineColor;
    }

    void Update()
    {
        if (!isInitialDropComplete)
        {
            if (isLaunchable)
            {
                isInitialDropComplete = true;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            return;
        }

        if (isLaunchable && Input.GetMouseButtonDown(0))
        {
            StartDragging();
        }

        if (isDragging)
        {
            ContinueDragging();

            if (Input.GetMouseButtonUp(0))
            {
                StopDragging();
            }
        }

        if (!IsInCameraBounds())
        {
            Respawn();
        }

        if (isLaunched && rb.velocity.magnitude < minimalMovementThreshold)
        {
            ResetLaunch();
        }
    }

    void StartDragging()
    {
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lineRenderer.positionCount = 2; // Ensure the line renderer has 2 positions
        isDragging = true;
        transform.SetParent(null);
    }

    void ContinueDragging()
    {
        endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DrawLine(startPos, endPos);
        DisplayTrajectory(startPos, endPos);
    }

    void StopDragging()
    {
        isDragging = false;
        if (Vector2.Distance(startPos, endPos) < minimalMovementThreshold)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            isLaunchable = true;
        }
        else
        {
            isLaunched = true;
            isLaunchable = false;
            rb.constraints = RigidbodyConstraints2D.None;
            ShootPlayer(startPos, endPos);
        }
        lineRenderer.positionCount = 0; // Hide the line
        HideTrajectory();
    }

    void DrawLine(Vector2 start, Vector2 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    void DisplayTrajectory(Vector2 start, Vector2 end)
    {
        // Add trajectory display logic here
    }

    void HideTrajectory()
    {
        // Add logic to hide trajectory
    }

    void ShootPlayer(Vector2 start, Vector2 end)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance, 0, maxLaunchForce);
        rb.velocity = force;
        transform.rotation = initialRotation;
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
        // Update respawn counter if necessary
    }

    void ResetLaunch()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        isLaunched = false;
        isLaunchable = true; // Allow launching again
    }
}
