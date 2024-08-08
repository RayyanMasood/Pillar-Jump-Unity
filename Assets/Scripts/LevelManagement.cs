using System.Collections;
using UnityEngine;

public class LevelManagement : MonoBehaviour
{
    public Transform player;
    public Transform MainCamera;
    public float slideDuration = 2f;
    public float spawnOffset = 2f; // Increased offset to spawn player above the first child pillar
    public int startLevelIndex = 0; // Added public variable to set the starting level index

    private Transform[] levels;
    private int currentLevelIndex;

    void Start()
    {
        // Initialize levels by ignoring the first child (Player)
        int levelCount = transform.childCount - 1;
        levels = new Transform[levelCount];
        for (int i = 0; i < levelCount; i++)
        {
            levels[i] = transform.GetChild(i + 1);
            levels[i].gameObject.SetActive(false);
        }

        // Ensure the startLevelIndex is within bounds
        startLevelIndex = Mathf.Clamp(startLevelIndex, 0, levels.Length - 1);
        currentLevelIndex = startLevelIndex;

        // Set the initial level and spawn the player and camera
        InitializeLevel(currentLevelIndex);

        // Start the game at the specified starting level
        StartCoroutine(StartLevel());
    }

    void InitializeLevel(int levelIndex)
    {
        Transform levelContainer = levels[levelIndex].Find("Level");
        if (levelContainer == null)
        {
            Debug.LogError($"Level container 'Level' not found in Level {levelIndex + 1}.");
            return;
        }

        // Activate the selected level
        levelContainer.gameObject.SetActive(true);

        // Set player position above the first child pillar of the current level
        Transform firstChild = levelContainer.childCount > 0 ? levelContainer.GetChild(0) : null;
        if (firstChild == null)
        {
            Debug.LogError($"First child of 'Level' not found in Level {levelIndex + 1}.");
            return;
        }

        player.position = firstChild.position + Vector3.up * spawnOffset;

        // Update initial position and rotation for the player
        var playerController = player.GetComponent<PlayerController_re>();
        if (playerController != null)
        {
            playerController.initialPosition = firstChild.position + Vector3.up * spawnOffset;
            playerController.initialRotation = player.rotation;
        }

        // Set the camera position to the center point of the current level
        Transform centerPoint = levelContainer.Find("CenterPoint");
        if (centerPoint == null)
        {
            Debug.LogError($"CenterPoint not found in Level {levelIndex + 1}.");
            return;
        }

        MainCamera.position = new Vector3(centerPoint.position.x, centerPoint.position.y, MainCamera.position.z);
    }

    IEnumerator StartLevel()
    {
        while (currentLevelIndex < levels.Length)
        {
            // Get the current level (LevelK -> Level)
            Transform levelContainer = levels[currentLevelIndex].Find("Level");
            if (levelContainer == null)
            {
                Debug.LogError($"Level container 'Level' not found in Level {currentLevelIndex + 1}.");
                yield break;
            }

            levelContainer.gameObject.SetActive(true);

            // Set the player position above the first child pillar of the current level
            Transform firstChild = levelContainer.childCount > 0 ? levelContainer.GetChild(0) : null;
            if (firstChild == null)
            {
                Debug.LogError($"First child of 'Level' not found in Level {currentLevelIndex + 1}.");
                yield break;
            }

            // Update initial position and rotation
            var playerController = player.GetComponent<PlayerController_re>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController_re component not found on player.");
                yield break;
            }

            playerController.initialPosition = firstChild.position + Vector3.up * spawnOffset;
            playerController.initialRotation = player.rotation;

            // Ensure the center point exists
            Transform centerPoint = levelContainer.Find("CenterPoint");
            if (centerPoint == null)
            {
                Debug.LogError($"CenterPoint not found in Level {currentLevelIndex + 1}.");
                yield break;
            }

            // Wait for the player to land on the final pillar
            yield return new WaitUntil(() => playerController.isLanded &&
                                            player.parent != null && player.parent.GetComponent<Pillar>().isFinal);

            // Detach player from the pillar
            player.SetParent(null);

            // Enable the next level before the transition
            if (currentLevelIndex + 1 < levels.Length)
            {
                levels[currentLevelIndex + 1].gameObject.SetActive(true);
            }

            // Move to the next level smoothly
            yield return StartCoroutine(TransitionToNextLevel());

            // Move to the next level index
            currentLevelIndex++;
        }

        // Game Completed
        Debug.Log("All levels completed!");
    }

    IEnumerator SlideCamera(Vector3 targetPosition)
    {
        Vector3 startPosition = MainCamera.position;
        Vector3 targetPositionAdjusted = new Vector3(targetPosition.x, targetPosition.y, startPosition.z);
        float elapsedTime = 0f;

        while (elapsedTime < slideDuration)
        {
            MainCamera.position = Vector3.Lerp(startPosition, targetPositionAdjusted, elapsedTime / slideDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        MainCamera.position = targetPositionAdjusted;
    }

    IEnumerator TransitionToNextLevel()
    {
        if (currentLevelIndex + 1 >= levels.Length)
        {
            yield break; // No more levels to transition to
        }

        // Get the next level center point
        Transform nextLevelContainer = levels[currentLevelIndex + 1].Find("Level");
        if (nextLevelContainer == null)
        {
            Debug.LogError($"Level container 'Level' not found in Level {currentLevelIndex + 2}.");
            yield break;
        }

        Transform nextFirstChild = nextLevelContainer.childCount > 0 ? nextLevelContainer.GetChild(0) : null;
        if (nextFirstChild == null)
        {
            Debug.LogError($"First child of 'Level' not found in Level {currentLevelIndex + 2}.");
            yield break;
        }

        Transform nextCenterPoint = nextLevelContainer.Find("CenterPoint");
        if (nextCenterPoint == null)
        {
            Debug.LogError($"CenterPoint not found in Level {currentLevelIndex + 2}.");
            yield break;
        }

        // Get the final pillar of the current level
        Transform currentLevelContainer = levels[currentLevelIndex].Find("Level");
        Transform finalPillar = null;
        for (int i = 0; i < currentLevelContainer.childCount; i++)
        {
            Pillar pillar = currentLevelContainer.GetChild(i).GetComponent<Pillar>();
            if (pillar != null && pillar.isFinal)
            {
                finalPillar = currentLevelContainer.GetChild(i);
                break;
            }
        }
        if (finalPillar == null)
        {
            Debug.LogError("Final pillar not found in the current level.");
            yield break;
        }

        // Calculate the position difference and move the next level
        Vector3 positionDifference = finalPillar.position - nextFirstChild.position;
        nextLevelContainer.position += positionDifference;

        // Update the positions of all pillars in the next level
        foreach (Transform child in nextLevelContainer)
        {
            PillarMovement pillarMovement = child.GetComponent<PillarMovement>();
            if (pillarMovement != null)
            {
                pillarMovement.InitializePositions();
            }
        }

        // Slide the camera to the next level's center point
        yield return StartCoroutine(SlideCamera(nextCenterPoint.position));

        // Update the player's initial position and rotation for respawning purposes
        var playerController = player.GetComponent<PlayerController_re>();
        if (playerController != null)
        {
            playerController.initialPosition = nextFirstChild.position + Vector3.up * spawnOffset;
            playerController.initialRotation = player.rotation;
        }

        // Disable the previous level after the transition
        levels[currentLevelIndex].gameObject.SetActive(false);
    }

    void Update()
    {
        // Ensure the current level is active
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i] != null && i == currentLevelIndex)
            {
                levels[i].gameObject.SetActive(true);
            }
        }

        // Ensure the player remains active
        if (player != null)
        {
            player.gameObject.SetActive(true);
        }
    }
}