using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public GameObject mainMenu; // The "Main" child in the canvas
    public GameObject levelSelection; // The "Level Selection" child in the canvas

    private void Start()
    {
        // Set the target frame rate to 120 fps
        Application.targetFrameRate = 120;

        PlayerPrefs.SetInt("SelectedLevel", 0);

        // Ensure the main menu is active and level selection is inactive at the start
        mainMenu.SetActive(true);
        levelSelection.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Levels()
    {
        // Activate the "Level Selection" child and deactivate the "Main" child
        levelSelection.SetActive(true);
        mainMenu.SetActive(false);
    }
}
