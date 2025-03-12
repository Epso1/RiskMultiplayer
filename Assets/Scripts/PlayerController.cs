using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    private Vector2 movement;
    public string playerName;
    [SerializeField] public Text playerNameText;
    void Update()
    {
        if (!IsOwner) return; // If not owner return
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized; // Create a movement vector with X and Y input axes
        transform.Translate(movement * moveSpeed * Time.deltaTime); // Move the player 
    }

    public void UpdatePlayerNameText()
    {
        playerNameText.text = playerName;
    }
}
