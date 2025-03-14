using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Collections;
using System.Linq;

public class NetworkUIManager : NetworkBehaviour
{
    [Header("Network UI")]
    [SerializeField] Button updatePlayersButton;
    [SerializeField] Button createGameButton;
    [SerializeField] Button joinGameButton;
    [SerializeField] Button disconnectButton;
    [SerializeField] Text playerIdsText;
    
    public NetworkVariable<FixedString128Bytes> playerIdsFixedString = new NetworkVariable<FixedString128Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private bool isNetworkSpawn = false;

    private void Awake()
    {
        createGameButton.onClick.AddListener(() => { TryStartHost(); });
        joinGameButton.onClick.AddListener(() => { TryStartClient(); });
        disconnectButton.onClick.AddListener(() => { TryShutdown(); });
        updatePlayersButton.onClick.AddListener(() => { TryUpdatePlayerIdsText(); });
    }
    public override void OnNetworkSpawn()
    {
        isNetworkSpawn = true;
    }
    private void TryStartHost()
    {        
        if (NetworkManager.Singleton.IsHost) // Host cannot start host.
        {
            Debug.Log("Cannot start host from host.");
        }
        else if (NetworkManager.Singleton.IsClient) // Client cannot start host.
        {
            Debug.Log("Cannot start host from client.");
        }
        else 
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started successfully.");
        }
    }

    private void TryStartClient()
    {
        if (NetworkManager.Singleton.IsHost) // Host cannot start client.
        {
            Debug.Log("Cannot start client from host.");
        }
        else if (NetworkManager.Singleton.IsClient) // Client cannot start client.
        {
            Debug.Log("Cannot start client from client.");
        }
        else // Start client
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started.");
        }       
    }
    private void TryShutdown()
    {
        NetworkManager.Singleton.Shutdown();
        Debug.Log("Player " + NetworkManager.Singleton.LocalClientId + " has been disconnected.");
    }

    // El parámetro ServerRpcParams nos permite conocer el cliente que realizó la petición.
    [ServerRpc(RequireOwnership = false)]
    public void RequestUpdatePlayerIdsTextServerRpc(ServerRpcParams serverRpcParams = default)
    {
        List<ulong> playerIds;
        ulong senderClientId = serverRpcParams.Receive.SenderClientId;  // Se obtiene el ID del cliente que hizo la llamada.

        // Crea una lista con las IDs de los clientes conectados.
        IReadOnlyList<ulong> connectedIds = NetworkManager.Singleton.ConnectedClientsIds; 
        playerIds = connectedIds.ToList();
     
        string updateText = "Connected Players:\n" + string.Join("\n", playerIds);  // Crea un string con saltos de línea a partir de la lista de IDs
        playerIdsFixedString.Value = updateText; // Asigna el string al valor la NetworkVariable playerIdsFixedString

        UpdatePlayerIdsTextTargetClientRpc(updateText, new ClientRpcParams // Se envía una actualización solo al cliente que hizo la solicitud
        {
            Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { senderClientId } }
        });
        Debug.Log("PlayerIDs text updated for client " + senderClientId + ": " + updateText);
    }

    // Esta ClientRpc recibe un ClientRpcParams para determinar a quién se le envía la actualización.
    [ClientRpc]
    private void UpdatePlayerIdsTextTargetClientRpc(string updatedText, ClientRpcParams clientRpcParams = default)
    {
        playerIdsText.text = updatedText;
    }


    // Método puente para controlar si se ha inicializado la red antes de actualizar el texto
    private void TryUpdatePlayerIdsText()
    {
        if (!isNetworkSpawn)
        {
            playerIdsText.text = "Player not connected.";
        }
        else RequestUpdatePlayerIdsTextServerRpc();
    }

}
