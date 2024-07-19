using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_re : MonoBehaviour
{
    public Rigidbody2D Player;
    public LineRenderer launchLine;
    public float maxLaunchForce = 20f;
    public float stickingMargin = 0.01f;
    public float lineWidth = 0.1f;  // Expose line width
    public Color lineColor = Color.white;  // Expose line color
    public float angleIncrement = 45f;  // Angle increment for rotation and alignment

    private Vector2 startPos;
    private Vector2 endPos;

    // states
    private bool isLanded = false;
    private bool isLaunched = false;
    private bool isDragging = false;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Camera mainCamera;

    // Reference to the last pillar touched
    private Collider2D lastPillarTouched;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        mainCamera = Camera.main;
        Player.constraints = RigidbodyConstraints2D.None;

        // Set initial line width
        launchLine.startWidth = lineWidth;
        launchLine.endWidth = lineWidth;

        // Ensure the material color is set
        launchLine.material.color = lineColor;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInCameraBounds())
        {
            ResetPlayerPosition();
        }

        if (!isLanded || isLaunched)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            launchLine.positionCount = 2; // Ensure the line renderer has 2 positions
            isDragging = true;
        }

        if (isDragging)
        {
            endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TrajectoryManager.Instance.DrawLine(startPos, endPos, launchLine, lineWidth, lineColor);
            TrajectoryManager.Instance.DisplayTrajectory(startPos, endPos, transform.position, Player, maxLaunchForce);
            UIManager.Instance.UpdateAimIndicator(TrajectoryManager.Instance.GetTrajectoryEndPosition());
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Player.constraints = RigidbodyConstraints2D.None;
            ShootPlayer(startPos, endPos);
            launchLine.positionCount = 0;
            TrajectoryManager.Instance.HideDots();
            isDragging = false; // Ensure dragging state is reset
        }
    }

    private void ShootPlayer(Vector2 start, Vector2 end)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance, 0, maxLaunchForce);
        Player.velocity = force;
        isLaunched = true;

        // Start the coroutine to check if the player has launched
        StartCoroutine(CheckIfLaunched());
    }

    private IEnumerator CheckIfLaunched()
    {
        // Wait for a short duration
        yield return new WaitForSeconds(0.02f);

        // Recheck the last trigger
        RecheckLastTrigger();

        
    }

    private void RecheckLastTrigger()
    {
        if (lastPillarTouched != null)
        {
            CheckAlignment(lastPillarTouched);
        }
    }

    private void CheckAlignment(Collider2D other)
    {
        // Calculate the number of increments needed for alignment check
        int increments = Mathf.RoundToInt(360f / angleIncrement);

        // Check if the player's rotation matches any of the acceptable angles within the margin of error
        bool isAligned = false;
        for (int i = 0; i < increments; i++)
        {
            float targetAngle = Mathf.DeltaAngle(transform.eulerAngles.z, other.transform.eulerAngles.z + i * angleIncrement);
            if (Mathf.Abs(targetAngle) < stickingMargin)
            {
                isAligned = true;
                break;
            }
        }

        if (isAligned)
        {
            Debug.Log("Proper angle");
            Player.constraints = RigidbodyConstraints2D.FreezeAll; // Freeze position and rotation
            isLanded = true;
            isLaunched = false;
            lastPillarTouched = other;

            // Align the player's rotation with the collider's rotation
            transform.rotation = Quaternion.Euler(0, 0, other.transform.eulerAngles.z);
        }
        else
        {
            Player.constraints = RigidbodyConstraints2D.None; // Allow the player to adjust position
        }
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
            Debug.Log("Touched top collider");
            //lastPillarTouched = other;
            CheckAlignment(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Top Collider") && other == lastPillarTouched)
        {
            lastPillarTouched = null;
        }
    }

    private void ResetPlayerPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        Player.velocity = Vector2.zero;
        Player.angularVelocity = 0f;
        Player.constraints = RigidbodyConstraints2D.None;
        GameManager.Instance.IncrementRespawnCount();
        isLanded = false;
    }

    // Method to rotate the player by angleIncrement degrees
    public void RotatePlayer()
    {
        transform.Rotate(0, 0, angleIncrement);
        RecheckLastTrigger(); // Recheck alignment after rotating
    }
}
