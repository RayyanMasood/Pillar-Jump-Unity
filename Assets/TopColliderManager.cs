using UnityEngine;

public class TopColliderManager : MonoBehaviour
{
    public float colliderWidth = 1.0f;

    void Start()
    {
        AdjustAllTopColliders();
    }

    void OnValidate()
    {
        AdjustAllTopColliders();
    }

    void AdjustAllTopColliders()
    {
        GameObject[] topColliders = GameObject.FindGameObjectsWithTag("Top Collider");

        foreach (GameObject topColliderObject in topColliders)
        {
            BoxCollider2D topCollider = topColliderObject.GetComponent<BoxCollider2D>();
            if (topCollider != null)
            {
                // Assuming the parent object is the pillar
                Transform parentTransform = topColliderObject.transform.parent;
                if (parentTransform != null)
                {
                    BoxCollider2D pillarCollider = parentTransform.GetComponent<BoxCollider2D>();
                    if (pillarCollider != null)
                    {
                        float pillarWidth = pillarCollider.size.x;
                        float adjustedWidth = Mathf.Min(colliderWidth, pillarWidth);
                        Vector2 size = topCollider.size;
                        size.x = adjustedWidth;
                        topCollider.size = size;
                    }
                }
            }
        }
    }
}
