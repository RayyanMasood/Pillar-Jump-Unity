using System.Collections;
using UnityEngine;

public class Person : MonoBehaviour
{
    private LevelManagement levelManagement;
    public AudioClip personSavedClip;

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

            if (personSavedClip != null)
            {
                // Create a new GameObject for audio playback
                GameObject audioPlayer = new GameObject("AudioPlayer");
                AudioSource audioSource = audioPlayer.AddComponent<AudioSource>();
                audioSource.PlayOneShot(personSavedClip);

                // Destroy the audio player after the clip finishes
                Destroy(audioPlayer, personSavedClip.length);

                Debug.Log("Playing sound: " + personSavedClip.name);
            }
            else
            {
                Debug.LogError("personSavedClip is not assigned.");
            }

            // Immediately deactivate and destroy the person object
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
