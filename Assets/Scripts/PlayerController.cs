using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    private Vector2 movement;
    public NetworkVariable<FixedString128Bytes> playerName;
    [SerializeField] public Text playerNameText;
    public override void OnNetworkSpawn()
    {
        // Suscribirse al callback para actualizar el texto cuando cambie el valor.
        playerName.OnValueChanged += OnPlayerNameChanged;
    
        // Actualizar la UI al spawnear por si el valor ya está seteado.
        UpdatePlayerNameText();
    }

    private void OnPlayerNameChanged(FixedString128Bytes oldName, FixedString128Bytes newName)
    {
        UpdatePlayerNameText();
    }
    void Update()
    {
        if (!IsOwner) return; // If not owner return
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized; // Create a movement vector with X and Y input axes
        transform.Translate(movement * moveSpeed * Time.deltaTime); // Move the player 
    }

    public void UpdatePlayerNameText()
    {
        playerNameText.text = playerName.Value.ToString();
    }

    [ClientRpc]
    public void SetSpawnPositionClientRpc(Vector3 spawnPos)
    {
        transform.position = spawnPos;
    }
}
