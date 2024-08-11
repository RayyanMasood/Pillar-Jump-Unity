using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MMBackButton : MonoBehaviour
{
    // Assign these in the Unity Inspector
    public GameObject mainMenu; // The "Main" child in the canvas
    public GameObject levelSelection; // The "Level Selection" child in the canvas
    public void OnButtonPressed()
    {
        // Activate the "Level Selection" child and deactivate the "Main" child
        levelSelection.SetActive(false);
        mainMenu.SetActive(true);
    }
}
