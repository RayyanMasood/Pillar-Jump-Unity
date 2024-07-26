using UnityEngine;

public class Pillar : MonoBehaviour
{
    public bool isFinal = false;
    public bool hasPersonToSave = false;
    public bool isActive = true;
    public Sprite personSprite;
    public GameObject personObject;

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


        if (hasPersonToSave)
        {
            CreatePerson();
        }
    }

    private void CreatePerson()
    {
        if (personSprite != null)
        {
            personObject = new GameObject("Person");
            SpriteRenderer personRenderer = personObject.AddComponent<SpriteRenderer>();
            personRenderer.sprite = personSprite;
            personObject.transform.SetParent(topCollider.transform);
            personObject.transform.localPosition = Vector3.zero;
        }
    }

    public void SavePerson()
    {
        if (hasPersonToSave && personObject != null)
        {
            Destroy(personObject);
            hasPersonToSave = false;
            // Implement score increment logic here
        }
    }

    private void UpdateIndicatorState()
    {
        if (isFinal && !isActive)
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
