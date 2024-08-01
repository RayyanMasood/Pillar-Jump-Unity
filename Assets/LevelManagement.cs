using System.Collections;
using UnityEngine;

public class LevelManagement : MonoBehaviour
{
    public Transform player;
    public Transform MainCamera;
    public float slideDuration = 2f;
    public float spawnOffset = 2f; // Increased offset to spawn player above the first child pillar

    private Transform[] levels;
    private int currentLevelIndex = 0;

    void Start()
    {
        // Initialize levels by ignoring the first child (Player)
        int levelCount = transform.childCount - 1;
        levels = new Transform[levelCount];
        for (int i = 0; i < levelCount; i++)
        {
            levels[i] = transform.GetChild(i + 1);
        }

        Transform levelContainer = levels[currentLevelIndex].Find("Level");
        Transform firstChild = levelContainer.childCount > 0 ? levelContainer.GetChild(0) : null;
        player.position = firstChild.position + Vector3.up * spawnOffset;

        // Start the game at the first level
        StartCoroutine(StartLevel());
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

            // Set the player position above the first child pillar of the current level
            Transform firstChild = levelContainer.childCount > 0 ? levelContainer.GetChild(0) : null;
            if (firstChild == null)
            {
                Debug.LogError($"First child of 'Level' not found in Level {currentLevelIndex + 1}.");
                yield break;
            }

            //player.position = firstChild.position + Vector3.up * spawnOffset;

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

            // Move camera to the center of the current level
            yield return StartCoroutine(SlideCamera(centerPoint.position));

            // Wait for the player to land on the final pillar
            yield return new WaitUntil(() => playerController.isLanded &&
                                            player.parent != null && player.parent.GetComponent<Pillar>().isFinal);

            // Detach player from the pillar
            player.SetParent(null);

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

        // Enable the next level before the transition
        levels[currentLevelIndex + 1].gameObject.SetActive(true);

        //player.SetParent(nextFirstChild);

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
                //Debug.Log("Setting " + i);
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
