using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject levelsContainer; // The "Levels" child containing all level buttons
    public Toggle unlockAllToggle; // The toggle for enabling all levels
    private List<Button> levelButtons = new List<Button>();
    private string saveFilePath;

    private void Start()
    {
        saveFilePath = Application.persistentDataPath + "/unlockedLevels.json";

        // Get all the buttons in the Levels container
        foreach (Transform levelButton in levelsContainer.transform)
        {
            Button button = levelButton.GetComponent<Button>();
            if (button != null)
            {
                levelButtons.Add(button);
            }
        }

        // Disable all buttons except the first one
        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (i == 0)
            {
                EnableButton(levelButtons[i], i);
            }
            else
            {
                DisableButton(levelButtons[i]);
            }
        }

        // Load and apply unlocked levels from the JSON file
        LoadUnlockedLevels();

        // Add listener for the unlock all toggle
        unlockAllToggle.onValueChanged.AddListener(delegate { ToggleAllLevels(unlockAllToggle.isOn); });
    }

    private void DisableButton(Button button)
    {
        button.interactable = false;
        ColorBlock cb = button.colors;
        cb.normalColor = Color.gray; // Set the button color to gray when disabled
        button.colors = cb;
    }

    private void ToggleAllLevels(bool isUnlocked)
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            if (isUnlocked || i == 0) // Ensure the first level remains unlocked
            {
                EnableButton(levelButtons[i], i);
            }
            else
            {
                DisableButton(levelButtons[i]);
            }
        }
    }

    private void EnableButton(Button button, int levelNumber)
    {
        button.interactable = true;
        ColorBlock cb = button.colors;
        cb.normalColor = Color.clear; // Set the button color back to white when enabled
        button.colors = cb;

        // Assign the LoadLevel function to the button's onClick event
        button.onClick.RemoveAllListeners(); // Remove any existing listeners to avoid duplicates
        button.onClick.AddListener(() => LoadLevel(levelNumber));
    }

    private void LoadLevel(int levelNumber)
    {
        // Save the selected level (optional, in case you need it in the next scene)
        GameProgress progress = new GameProgress
        {
            currentLevel = levelNumber,
            respawnCounter = 0
        };
        SaveSystem.SaveProgress(progress);

        // Load the next scene in the build order
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("There is no next scene in the build order.");
        }
    }

    private void LoadUnlockedLevels()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            UnlockedLevels unlockedLevels = JsonUtility.FromJson<UnlockedLevels>(json);

            if (unlockedLevels != null && unlockedLevels.unlockedLevelIndices != null)
            {
                foreach (int index in unlockedLevels.unlockedLevelIndices)
                {
                    if (index > 0 && index < levelButtons.Count)
                    {
                        EnableButton(levelButtons[index], index);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No unlockedLevels.json file found. Only the first level will be unlocked.");
        }
    }
}
