using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movement;

    void Update()
    {
        if (!IsOwner) return; // If not owner return
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized; // Create a movement vector with X and Y input axes
        transform.Translate(movement * moveSpeed * Time.deltaTime); // Move the player 
    }
}
