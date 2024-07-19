using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TextMeshProUGUI respawnCounterText;
    public Image aimIndicator;

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

    public void UpdateRespawnCounter(int respawnCount)
    {
        respawnCounterText.text = respawnCount.ToString();
    }

    public void UpdateAimIndicator(Vector2 arcEndPosition)
    {
        float closestDistance = float.MaxValue;
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(arcEndPosition, 10f))
        {
            if (collider.CompareTag("Top Collider"))
            {
                float distance = Vector2.Distance(arcEndPosition, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
        }

        float maxDistance = 5f;
        float t = Mathf.Clamp01(closestDistance / maxDistance);
        aimIndicator.color = Color.Lerp(Color.green, Color.red, t);
    }
}
