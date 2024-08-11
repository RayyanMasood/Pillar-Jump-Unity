using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public GameObject levelsContainer; // The "Levels" child containing all level buttons
    public Toggle unlockAllToggle; // The toggle for enabling all levels

    private List<Button> levelButtons = new List<Button>();

    private void Start()
    {
        // Get all the buttons in the Levels container
        foreach (Transform levelButton in levelsContainer.transform)
        {
            Button button = levelButton.GetComponent<Button>();
            if (button != null)
            {
                levelButtons.Add(button);
                DisableButton(button);
            }
        }

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
            if (isUnlocked)
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
        PlayerPrefs.SetInt("SelectedLevel", levelNumber);

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
}
