using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonScript : MonoBehaviour
{
    public void OnBackButtonPressed()
    {

        // Get the current active scene's build index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Calculate the previous scene index
        int previousSceneIndex = currentSceneIndex - 1;

        GameObject managerObj = GameObject.Find("Managers");

        // Ensure the previous scene index is valid
        if (previousSceneIndex >= 0)
        {
            // Load the previous scene
            Destroy(managerObj);
            SceneManager.LoadScene(previousSceneIndex);
            
        
        }
        else
        {
            Debug.LogWarning("No previous scene in build settings.");
        }
    }

    public void AppQuit()
    {
        Application.Quit();
    }
        
    
}
