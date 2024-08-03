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
    public bool isLanded = false;
    private bool isLaunched = false;
    private bool isDragging = false;
    private bool inInfluence = false;

    public Vector3 initialPosition;
    public Quaternion initialRotation;
    private Camera mainCamera;
    public float initialGravity;

    // Reference to the last pillar touched
    private Collider2D lastPillarTouched;

    // Audio source for playing landing sounds
    private AudioSource audioSource;

    // Array to hold landing sounds
    public AudioClip[] landingSounds;

    // List to hold preloaded audio clips
    private List<AudioClip> preloadedLandingSounds;

    // Start is called before the first frame update
    void Start()
    {
        initialGravity = Player.gravityScale;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        mainCamera = Camera.main;
        Player.constraints = RigidbodyConstraints2D.None;

        // Set initial line width
        launchLine.startWidth = lineWidth;
        launchLine.endWidth = lineWidth;

        // Ensure the material color is set
        launchLine.material.color = lineColor;

        // Initialize the audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // Ensure the audio source does not play on awake
        audioSource.spatialBlend = 0f; // Ensure the audio is 2D and not affected by 3D spatial settings

        // Load all landing sounds from the Resources folder
        landingSounds = Resources.LoadAll<AudioClip>("Sounds/Landing");

        // Preload audio clips into memory
        preloadedLandingSounds = new List<AudioClip>(landingSounds.Length);
        foreach (var clip in landingSounds)
        {
            clip.LoadAudioData();
            preloadedLandingSounds.Add(clip);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsInCameraBounds())
        {
            Respawn();
        }

        if (!isLanded || isLaunched)
        {
            if (Input.GetMouseButtonDown(0) && !isDragging)
            {
                RotatePlayer();
            }
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
            Vector2 hitPosition = TrajectoryManager.Instance.DisplayTrajectory(startPos, endPos, transform.position, Player, maxLaunchForce);
            UIManager.Instance.UpdateAimIndicator(hitPosition);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            Player.constraints = RigidbodyConstraints2D.None;
            ShootPlayer(startPos, endPos);
            launchLine.positionCount = 0;
            TrajectoryManager.Instance.HideDots(); // Ensure dots are hidden after shooting
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
        isLanded = false;

        // Detach from the parent pillar
        transform.SetParent(null);

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
        // Get the Pillar component from the top_collider's parent
        Pillar pillar = other.transform.parent.GetComponent<Pillar>();
        if (pillar != null && !pillar.isActive)
        {
            Respawn();
            return;
        }

        // Calculate the player's current rotation and the pillar's top rotation
        float playerRotation = Mathf.Round(transform.eulerAngles.z);
        float pillarTopRotation = Mathf.Round(other.transform.eulerAngles.z);

        Debug.Log("Player Rotation at time of contact: " + playerRotation + " | Pillar Collider rotation: " + pillarTopRotation);

        // Define the sets of acceptable angles
        HashSet<float> set1 = new HashSet<float> { 0, 90, 180, 270, 360 };
        HashSet<float> set2 = new HashSet<float> { 45, 135, 225, 315 };

        // Determine which set the pillar's top rotation belongs to
        HashSet<float> acceptableAngles = set1.Contains(pillarTopRotation) ? set1 : set2.Contains(pillarTopRotation) ? set2 : null;

        Debug.Log("Acceptable Ranges: " + (acceptableAngles != null ? string.Join(", ", acceptableAngles) : "None"));

        if (acceptableAngles == null)
        {
            Player.constraints = RigidbodyConstraints2D.None; // Allow the player to adjust position
            return;
        }

        // Check if the player's rotation matches any of the extended acceptable angles within the margin of error
        bool isAligned = false;

        foreach (float angle in acceptableAngles)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(playerRotation, angle)) <= stickingMargin)
            {
                isAligned = true;
                break;
            }
        }

        if (isAligned)
        {
            // Play a random landing sound immediately
            PlayRandomLandingSound();
            Debug.Log("Proper angle");
            Player.constraints = RigidbodyConstraints2D.FreezeAll; // Freeze position and rotation
            isLanded = true;
            isLaunched = false;
            lastPillarTouched = other;

            // Align the player's rotation with the closest acceptable angle
            transform.rotation = Quaternion.Euler(0, 0, Mathf.RoundToInt(playerRotation / angleIncrement) * angleIncrement);

            // Set the player as a child of the pillar
            transform.SetParent(other.transform.parent);
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
            CheckAlignment(other);
        }
        if (other.CompareTag("HookInfluence"))
        {
            inInfluence = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Top Collider") && other == lastPillarTouched)
        {
            lastPillarTouched = null;
        }
        if (other.CompareTag("HookInfluence"))
        {
            inInfluence = false;
        }
    }

    private void Respawn()
    {
        transform.SetPositionAndRotation(initialPosition, initialRotation);
        Player.velocity = Vector2.zero;
        Player.angularVelocity = 0f;
        Player.gravityScale = initialGravity;
        Debug.Log("Set gravity back");

        Player.constraints = RigidbodyConstraints2D.None;
        transform.SetParent(null); // Detach from any parent
        GameManager.Instance.IncrementRespawnCount();
        isLanded = false;
    }

    // Method to rotate the player by angleIncrement degrees
    public void RotatePlayer()
    {
        if (!inInfluence)
        {
            transform.Rotate(0, 0, angleIncrement);
        }
    }

    private void PlayRandomLandingSound()
    {
        if (preloadedLandingSounds.Count > 0)
        {
            int randomIndex = Random.Range(0, preloadedLandingSounds.Count);
            audioSource.clip = preloadedLandingSounds[randomIndex];
            audioSource.Play();
        }
    }
}
