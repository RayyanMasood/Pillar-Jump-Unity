using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveManager : MonoBehaviour
{
    public LevelManagement levelManagement; // Reference to the LevelManagement script
    public Button backButton; // Reference to the Back button
    public TextMeshProUGUI respawnCounterText; // Reference to the respawnCounter UI element

    private int respawnCounter = 0;
    private string saveFilePath;

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/currentProgress.json";
    }

    private void Start()
    {
        // Load the saved progress at the start of the game
        LoadGameProgress();

        // Add listener to the back button
        backButton.onClick.AddListener(OnBackButtonPressed);
    }

    private void LoadGameProgress()
    {
        GameProgress progress = SaveSystem.LoadProgress();

        // Set the current level and respawn counter from the saved progress
        if (progress != null)
        {
            //levelManagement.startLevelIndex = progress.currentLevel > 0 ? progress.currentLevel : SceneManager.GetActiveScene().buildIndex;
            respawnCounter = progress.respawnCounter;
            respawnCounterText.text = respawnCounter.ToString();
        }

    }

    public void SaveGameProgress()
    {
        GameProgress progress = new GameProgress
        {
            currentLevel = levelManagement.currentLevelIndex,
            respawnCounter = int.Parse(respawnCounterText.text)
        };

        SaveSystem.SaveProgress(progress);
        Debug.Log(SaveSystem.LoadProgress().currentLevel + " " + SaveSystem.LoadProgress().respawnCounter);
    }

    private void SaveProgressToFile(GameProgress progress)
    {
        string json = JsonUtility.ToJson(progress);
        File.WriteAllText(saveFilePath, json);
    }

    private void OnApplicationQuit()
    {
        // Save progress when the application quits
        SaveGameProgress();
    }

    private void OnBackButtonPressed()
    {
        // Save progress and return to the main menu or previous scene
        
        SaveGameProgress();
        Debug.Log("From back");
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        
    }
}
