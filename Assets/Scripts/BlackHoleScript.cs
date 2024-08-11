using UnityEngine;

public class BlackHole : MonoBehaviour
{
    public Transform player;
    Rigidbody2D playerBody;
    public float influenceRange;
    public float intensity;
    public float distanceToPlayer;
    Vector2 pullForce;
    public Vector2 offset; // Add an offset variable

    void Start()
    {
        playerBody = player.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Apply the offset to the black hole's position
        Vector2 blackHolePosition = (Vector2)transform.position + offset;

        // Calculate the distance between the player and the adjusted black hole position
        distanceToPlayer = Vector2.Distance(player.position, blackHolePosition);

        if (distanceToPlayer <= influenceRange)
        {
            // Calculate the pull force towards the adjusted black hole position
            pullForce = (blackHolePosition - (Vector2)player.position).normalized / distanceToPlayer * intensity;

            // Apply the pull force to the player's Rigidbody2D component
            playerBody.AddForce(pullForce, ForceMode2D.Force);
        }
    }
}
