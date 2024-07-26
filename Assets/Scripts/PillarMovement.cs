using UnityEngine;

public class PillarMovement : MonoBehaviour
{
    public float moveAngle = 0f;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;

    private void Start()
    {
        startPosition = transform.position;
        Vector3 direction = new Vector3(Mathf.Cos(moveAngle * Mathf.Deg2Rad), Mathf.Sin(moveAngle * Mathf.Deg2Rad), 0);
        endPosition = startPosition + direction * moveDistance;
    }

    private void Update()
    {
        if (movingToEnd)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, endPosition) < 0.1f)
            {
                movingToEnd = false;
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, startPosition) < 0.1f)
            {
                movingToEnd = true;
            }
        }
    }
}
