using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnManager : NetworkBehaviour
{
    public NetworkVariable<int> spawnIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
        int spawnIndexTemp = (int)clientId;
        spawnIndex.Value = spawnIndexTemp;
        Vector3 spawnPosition = spawnPoints[spawnIndex.Value].position;

        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        PlayerController playerController = playerObject.GetComponent<PlayerController>();
        playerController.playerName.Value = "Player" + clientId;
        playerObject.transform.position = spawnPosition; // Actualiza en el servidor
        Debug.Log($"Jugador {playerController.playerName.Value} spawneado en: {spawnPosition}");

        // Notifica al cliente para actualizar su posición
        playerController.SetSpawnPositionClientRpc(spawnPosition);
    }


}
