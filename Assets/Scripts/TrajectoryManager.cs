using System.Collections.Generic;
using UnityEngine;

public class TrajectoryManager : MonoBehaviour
{
    public static TrajectoryManager Instance { get; private set; }
    public GameObject dotPrefab;
    public int numberOfDots = 20;
    public float dotSpacing = 0.1f;
    public float dotTransparency = 0.5f;

    private List<GameObject> trajectoryDots = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateDots();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CreateDots()
    {
        for (int i = 0; i < numberOfDots; i++)
        {
            GameObject dot = Instantiate(dotPrefab);
            dot.SetActive(false);
            Color dotColor = dot.GetComponent<SpriteRenderer>().color;
            dotColor.a = dotTransparency;
            dot.GetComponent<SpriteRenderer>().color = dotColor;
            trajectoryDots.Add(dot);
        }
    }

    public void DrawLine(Vector2 start, Vector2 end, LineRenderer lineRenderer, float lineWidth, Color lineColor)
    {
        lineRenderer.positionCount = 2; // Ensure the line renderer has 2 positions
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.material.color = lineColor; // Ensure the material color is also set
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    public void DisplayTrajectory(Vector2 start, Vector2 end, Vector2 initialPosition, Rigidbody2D rb, float maxLaunchForce)
    {
        Vector2 direction = start - end;
        float distance = direction.magnitude;
        Vector2 force = direction.normalized * Mathf.Clamp(distance, 0, maxLaunchForce);

        Vector2 pos = initialPosition;
        Vector2 velocity = force;

        for (int i = 0; i < numberOfDots; i++)
        {
            float t = i * dotSpacing;
            Vector2 dotPos = pos + velocity * t + 0.5f * Physics2D.gravity * (t * t);
            trajectoryDots[i].SetActive(true);
            trajectoryDots[i].transform.position = dotPos;
        }
    }

    public void HideDots()
    {
        foreach (var dot in trajectoryDots)
        {
            dot.SetActive(false);
        }
    }

    public Vector2 GetTrajectoryEndPosition()
    {
        return trajectoryDots[numberOfDots - 1].transform.position;
    }
}
