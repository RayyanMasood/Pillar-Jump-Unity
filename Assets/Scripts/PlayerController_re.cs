using System.Collections;
using UnityEngine;

public class PlayerController_re : MonoBehaviour
{
    public Rigidbody2D Player;
    public LineRenderer launchLine;
    public float maxLaunchForce = 20f;
    public float stickingMargin = 0.01f;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.white;
    public float angleIncrement = 45f;

    private Vector2 startPos;
    private Vector2 endPos;

    private bool isLanded = false;
    private bool isLaunched = false;
    private bool isDragging = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Camera mainCamera;

    private Collider2D lastPillarTouched;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        mainCamera = Camera.main;
        Player.constraints = RigidbodyConstraints2D.None;
        Player.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        launchLine.startWidth = lineWidth;
        launchLine.endWidth = lineWidth;
        launchLine.material.color = lineColor;
    }

    void Update()
    {
        if (!IsInCameraBounds())
        {
            Debug.Log("Player out of camera bounds. Respawning...");
            Respawn();
        }

        if (!isLanded || isLaunched)
        {
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                Debug.Log("Rotating player...");
                RotatePlayer();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            launchLine.positionCount = 2;
            isDragging = true;
            Debug.Log("Start dragging at position: " + startPos);
        }

        if (isDragging)
        {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TrajectoryManager.Instance.DrawLine(startPos, endPos, launchLine, lineWidth, lineColor);
            TrajectoryManager.Instance.DisplayTrajectory(startPos, endPos, transform.position, Player, maxLaunchForce);
            UIManager.Instance.UpdateAimIndicator(TrajectoryManager.Instance.GetTrajectoryEndPosition());
            Debug.Log("Dragging to position: " + endPos);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Player.constraints = RigidbodyConstraints2D.None;
            ShootPlayer(startPos, endPos);
            launchLine.positionCount = 0;
            TrajectoryManager.Instance.HideDots();
            isDragging = false;
            Debug.Log("Shoot player from: " + startPos + " to: " + endPos);
        }
    }

    private void ShootPlayer(Vector2 start, Vector2 end)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance, 0, maxLaunchForce);
        Player.velocity = force;
        isLaunched = true;
        Debug.Log("Player launched with force: " + force);

        StartCoroutine(CheckIfLaunched());
    }

    private IEnumerator CheckIfLaunched()
    {
        yield return new WaitForSeconds(0.02f);
        RecheckLastTrigger();
    }

    private void RecheckLastTrigger()
    {
        if (lastPillarTouched != null)
        {
            Debug.Log("Rechecking alignment with the last pillar touched.");
            CheckAlignment(lastPillarTouched);
        }
    }

    private void CheckAlignment(Collider2D other)
    {
        Vector2 contactPoint = other.ClosestPoint(transform.position);
        Vector2 direction = contactPoint - (Vector2)transform.position;
        Vector2 localNormal = transform.InverseTransformDirection(direction.normalized);

        float angle = Mathf.Atan2(localNormal.y, localNormal.x) * Mathf.Rad2Deg;
        bool isFlatContact = Mathf.Abs(Mathf.Abs(angle) % 90) <= stickingMargin || Mathf.Abs(Mathf.Abs(angle) % 90 - 90) <= stickingMargin;

        Debug.Log("Contact point: " + contactPoint + ", Direction: " + direction + ", Local normal: " + localNormal + ", Angle: " + angle + ", Flat contact: " + isFlatContact);

        if (isFlatContact)
        {
            StickToSurface(other);
        }
        else
        {
            Player.constraints = RigidbodyConstraints2D.None;
            Debug.Log("Not a flat contact. Player can adjust position.");
        }
    }

    private void StickToSurface(Collider2D surface)
    {
        Player.velocity = Vector2.zero;
        Player.angularVelocity = 0;
        Player.constraints = RigidbodyConstraints2D.FreezeAll;
        isLanded = true;
        isLaunched = false;
        lastPillarTouched = surface;

        Debug.Log("Sticking to surface: " + surface.name);
        AlignRotation(surface.transform);
    }

    private void AlignRotation(Transform surfaceTransform)
    {
        float surfaceRotation = surfaceTransform.eulerAngles.z;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Round(surfaceRotation / angleIncrement) * angleIncrement);
        Debug.Log("Aligning rotation to surface. Surface rotation: " + surfaceRotation + ", Player rotation: " + transform.rotation.eulerAngles.z);
    }

    private bool IsInCameraBounds()
    {
        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);
        return viewportPosition.x >= -0.05 && viewportPosition.x <= 1.05 && viewportPosition.y >= -0.05 && viewportPosition.y <= 1.05;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Top Collider"))
        {
            lastPillarTouched = other;
            Debug.Log("Entered trigger with: " + other.name);
            CheckAlignment(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Top Collider") && other == lastPillarTouched)
        {
            Debug.Log("Exited trigger with: " + other.name);
            lastPillarTouched = null;
        }
    }

    private void Respawn()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        Player.velocity = Vector2.zero;
        Player.angularVelocity = 0f;
        Player.constraints = RigidbodyConstraints2D.None;
        GameManager.Instance.IncrementRespawnCount();
        isLanded = false;
        Debug.Log("Player respawned to initial position and rotation.");
    }

    public void RotatePlayer()
    {
        transform.Rotate(0, 0, angleIncrement);
        Debug.Log("Player rotated to: " + transform.rotation.eulerAngles.z);
    }
}
