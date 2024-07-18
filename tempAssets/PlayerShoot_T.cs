using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShoot_T : MonoBehaviour
{
    public Rigidbody2D rb;
    public LineRenderer lineRenderer;
    public GameObject dotPrefab;
    public int numberOfDots = 20;
    public float maxLaunchForce = 20f;
    public float lineWidth = 0.1f;
    public float dotSpacing = 0.1f;
    public float dotTransparency = 0.5f;
    public float minimalMovementThreshold = 0.01f;

    // UI elements
    public TextMeshProUGUI respawnCounterText;
    public Image aimIndicator;

    private Vector2 startPos;
    private Vector2 endPos;
    private bool isDragging = false;
    private bool isLaunched = false;
    private bool isLaunchable = false;
    private bool isInitialDropComplete = false;
    private bool isCorrectingRotation = false;
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

        rb.constraints = RigidbodyConstraints2D.None;

        UpdateRespawnCounter();
        aimIndicator.color = Color.red;
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

        HandleInput();
        CheckBoundsAndRespawn();
        CheckMinimalMovement();
    }

    void HandleInput()
    {
        if (isLaunchable && Input.GetMouseButtonDown(0))
        {
            StartDragging();
        }

        if (isDragging)
        {
            Dragging();
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDragging();
        }
    }

    void StartDragging()
    {
        startPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        lineRenderer.positionCount = 2;
        isDragging = true;
    }

    void Dragging()
    {
        endPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        DrawLine(startPos, endPos);
        DisplayTrajectory(startPos, endPos);
        UpdateAimIndicator();
    }

    void EndDragging()
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
        lineRenderer.positionCount = 0;
        HideDots();
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

        transform.rotation = initialRotation;
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
        Debug.Log("Is Flat Side Aligned: " + IsFlatSideAligned(other));
        if (other.CompareTag("Top Collider") && IsFlatSideAligned(other))
        {
            isLaunchable = true;
            if (isLaunched)
            {
                StartRotationCorrection();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Top Collider"))
        {
            isLaunchable = false;
        }
    }

    bool IsFlatSideAligned(Collider2D collider)
    {
        Vector2 collisionPoint = collider.ClosestPoint(transform.position);
        Vector2 directionToCollider = (collisionPoint - (Vector2)transform.position).normalized;
        float angle = Vector2.Angle(transform.up, directionToCollider) - 90;
        Debug.Log("Checking the angle of the top collider to player: " + angle);

        // Check if the player is aligned with flat sides (0, 90, 180, 270 degrees)
        float rotationZ = transform.eulerAngles.z;
        bool isAligned = myApproximation(rotationZ % 90, 0, 0.05f);
        Debug.Log("Is the player at 90 degree angles?: " + rotationZ + "; " + isAligned);
        Debug.Log("Is the angle of the collider at 90 degree angles?: " + (myApproximation(angle, 90, 0.05f) || myApproximation(angle, 270, 0.05f)));

        // Check if the angle to the collider is perpendicular (90 or 270 degrees)
        bool isPerpendicular = myApproximation(angle, 90, 0.05f) || myApproximation(angle, 270, 0.05f);

        return isAligned && isPerpendicular;
    }

    private bool myApproximation(float a, float b, float tolerance)
    {
        return (Mathf.Abs(a - b) < tolerance);
    }

    void StartRotationCorrection()
    {
        isCorrectingRotation = true;
        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition;
        isLaunched = false;
    }

    bool IsInCameraBounds()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.x >= -0.05f && viewportPosition.x <= 1.05f && viewportPosition.y >= -0.05f && viewportPosition.y <= 1.05f;
    }

    void CheckBoundsAndRespawn()
    {
        if (isLaunched && !IsInCameraBounds())
        {
            Respawn();
        }
    }

    void Respawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.None;
        isLaunched = false;
        isLaunchable = false;
        isInitialDropComplete = false;
        respawnCount++;
        UpdateRespawnCounter();
    }

    void CheckMinimalMovement()
    {
        if (isLaunched && rb.velocity.magnitude < minimalMovementThreshold)
        {
            ResetLaunch();
        }
    }

    void ResetLaunch()
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        isLaunched = false;
        isLaunchable = true;
    }

    void UpdateRespawnCounter()
    {
        respawnCounterText.text = "Respawns: " + respawnCount;
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

        float maxDistance = 5f;
        float t = Mathf.Clamp01(closestDistance / maxDistance);
        aimIndicator.color = Color.Lerp(Color.green, Color.red, t);
    }
}
