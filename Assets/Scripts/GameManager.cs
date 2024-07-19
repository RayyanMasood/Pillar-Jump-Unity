using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int RespawnCount { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void IncrementRespawnCount()
    {
        RespawnCount++;
        UIManager.Instance.UpdateRespawnCounter(RespawnCount);
    }
}
