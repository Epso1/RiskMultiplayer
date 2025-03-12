using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

// GameManager.cs: Maneja la conexión de jugadores y el inicio de la partida
public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> connectedPlayers = new NetworkVariable<int>(0);
    public NetworkVariable<FixedString128Bytes> gameName = new NetworkVariable<FixedString128Bytes>("");
    public NetworkList<FixedString128Bytes> playerNames;
    public int requiredPlayers = 3;
    public GameObject playerPrefab;

    private LobbyManager lobbyManager;

    private void Awake()
    {
        playerNames = new NetworkList<FixedString128Bytes>();       
    }

    private void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
        gameName.OnValueChanged += lobbyManager.UpdateGameNameText;
        //playerNames.OnListChanged += lobbyManager.UpdatePlayerList;
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            connectedPlayers.OnValueChanged += (oldValue, newValue) =>
            {
                if (newValue >= requiredPlayers)
                {
                    StartGame();
                }
            };
        }
        NetworkManager.SceneManager.OnLoadComplete += (clientId, sceneName, _) =>
        {
            if (sceneName == "GameScene" && IsServer)
            {
                SpawnPlayers();
            }
        };
    }

    public void SetupGame(string name, int playerCount)
    {
        gameName.Value = name;
        requiredPlayers = playerCount;
        connectedPlayers.Value = 1;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(string playerName)
    {
        if (connectedPlayers.Value >= requiredPlayers)
        {
            lobbyManager.PrintInfoMessage("Game is full. Cannot join.");
            return;
        }
        FixedString64Bytes fixedPlayerName = playerName;

        if (!playerNames.Contains(fixedPlayerName))
        {
            playerNames.Add(fixedPlayerName);
            connectedPlayers.Value++;
            lobbyManager.PrintInfoMessage($"{playerName} joined the game. Total players: {connectedPlayers.Value}/{requiredPlayers}");
        }
    }

    void StartGame()
    {
        lobbyManager.PrintInfoMessage("All players connected. Starting game!");
    }

    void SpawnPlayers()
    {
        foreach (var clientId in NetworkManager.ConnectedClientsIds)
        {
            Vector3 spawnPosition = GetSpawnPosition(clientId);
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }

    Vector3 GetSpawnPosition(ulong clientId)
    {
        float offset = clientId * 2.0f;
        return new Vector3(offset, 0, 0);
    }
}
