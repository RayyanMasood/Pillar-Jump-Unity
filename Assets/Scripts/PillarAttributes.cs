using UnityEngine;

public class Pillar : MonoBehaviour
{
    public bool isFinal = false;
    public bool isActive = true;
  

    private Collider2D topCollider;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color finalColor = Color.grey;

    private GameObject sideIndicator;
    private GameObject crossMark;

    private void Awake()
    {
        topCollider = transform.Find("Top_Collider").GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Find SideIndicator and CrossMark children
        sideIndicator = transform.Find("Pillar_SideIndicator").gameObject;
        crossMark = transform.Find("CrossMark").gameObject;

        UpdateIndicatorState();


    }

    private void Update()
    {
        UpdateIndicatorState();
    }



    private void UpdateIndicatorState()
    {
        if (!isActive)
        {
            spriteRenderer.color = finalColor;
            sideIndicator.SetActive(false);
            crossMark.SetActive(true);
            
        } else if (isActive)
        {
            spriteRenderer.color = originalColor;
            sideIndicator.SetActive(true);
            crossMark.SetActive(false);
        }
    }

    public void SetActiveState(bool active)
    {
        isActive = active;
        UpdateIndicatorState();
    }
}
