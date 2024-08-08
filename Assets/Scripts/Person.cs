using UnityEngine;

public class Person : MonoBehaviour
{
    private LevelManagement levelManagement;

    private void Start()
    {
        levelManagement = FindObjectOfType<LevelManagement>();
        if (levelManagement == null)
        {
            Debug.LogError("LevelManagement not found in the scene.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            levelManagement.PersonSaved(this);
            Destroy(gameObject);
        }
    }
}
