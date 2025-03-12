using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    // Arrastra tus puntos de spawn desde el Inspector.
    public Transform[] spawnPoints;

    private void Start()
    {  
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; // Suscribirse al evento de conexión de clientes.
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer) { FindObjectOfType<TurnManager>().UpdateTurnTextServerRpc(); } // Si es el host, actualiza el texto del turno.
        OnClientConnectedServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectedServerRpc(ulong clientId)
    {
        // Selecciona un spawn point de forma cíclica
        int spawnIndex = (int)(clientId % (ulong)spawnPoints.Length);
        Vector3 spawnPosition = spawnPoints[spawnIndex].position;

        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject; // Obtener el objeto jugador del cliente
        PlayerController playerController = playerObject.GetComponent<PlayerController>(); // Obtener el playerController del objeto del jugador
        playerController.playerName = "Player" + clientId; // Le damos un nombre al jugador
        playerObject.transform.position = spawnPosition; // Asigna la posición de spawn al jugador
        Debug.Log($"Jugador {playerController.playerName} spawneado en: {spawnPosition}");
        OnClientConnectedClientRpc(clientId);     
    }

    [ClientRpc]
    public void OnClientConnectedClientRpc(ulong clientId)
    {
         foreach (PlayerController playerCon in FindObjectsOfType<PlayerController>())
        {
            playerCon.UpdatePlayerNameText();
        }
    }
}
