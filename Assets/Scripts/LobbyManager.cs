using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// LobbyManager.cs: Gestiona la creación y unión a partidas
public class LobbyManager : NetworkBehaviour
{
    public Button updatePlayersButton;
    public Button createGameButton;
    public Button joinGameButton;
    public Button disconnectButton;
    public GameObject createGamePanel;
    public GameObject playerListPanel;
    public GameObject joinGamePanel;
    public InputField playerNameInput;
    public InputField gameNameInput;
    public InputField joinPlayerNameInputField;
    public Dropdown playerCountDropdown;
    public Text gameNameText;
    public Text playerListText;
    public Text infoText;

    private GameManager gameManager;
    private string gameName;
    private string playerName;

    void Start()
    {
        updatePlayersButton.onClick.AddListener(UpdatePlayerListServerRpc);
        createGameButton.onClick.AddListener(TryCreateGame);
        joinGameButton.onClick.AddListener(PrepareJoinGame);
        disconnectButton.onClick.AddListener(Disconnect);
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager != null)
        {
            gameManager.playerNames.OnListChanged += (changeEvent) => UpdatePlayerListServerRpc();
            gameManager.gameName.OnValueChanged += (oldValue, newValue) => UpdatePlayerListServerRpc();
        }
    }

    void TryCreateGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            PrintInfoMessage("A game is already created.");
            return;
        }
        createGamePanel.SetActive(true);
    }

    public void CreateGame()
    {
        playerName = playerNameInput.text;
        gameName = gameNameInput.text;
        int playerCount = int.Parse(playerCountDropdown.options[playerCountDropdown.value].text);

        NetworkManager.Singleton.StartHost();
        gameManager.SetupGame(gameName, playerCount);
        gameManager.AddPlayerServerRpc(playerName);
        createGamePanel.SetActive(false);
        PrintInfoMessage($"Game '{gameName}' created with {playerCount} players. Host: {playerName}");
        UpdatePlayerListServerRpc();
    }

    void PrepareJoinGame()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            PrintInfoMessage("You are already connected to a game.");
            return;
        }

        if (string.IsNullOrEmpty(gameManager.gameName.Value.ToString()))
        {
            PrintInfoMessage("No game has been created yet.");
            return;
        }
        joinGamePanel.SetActive(true);
    }

    public void PrintInfoMessage(string message)
    {
        infoText.text = message;
    }

    public void ConfirmJoinGame()
    {
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StartClient();
        }
        playerName = joinPlayerNameInputField.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            gameManager.AddPlayerServerRpc(playerName);
            joinGamePanel.SetActive(false);
        }
        else
        {
            PrintInfoMessage("Player name cannot be empty.");
        }
    }

    public void UpdatePlayerList(NetworkList<FixedString128Bytes> newValue)
    {      
        playerListText.text = "Players:\n" + string.Join("\n", newValue);
    }

    public void UpdateGameNameText(FixedString128Bytes oldValue, FixedString128Bytes newValue)
    {
        gameNameText.text = newValue.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerListServerRpc()
    {
        if (gameManager == null || gameManager.playerNames == null) return;

        List<string> playerNames = new List<string>();
        foreach (var playerName in gameManager.playerNames)
        {
            playerNames.Add(playerName.ToString());
        }
        playerListText.text = "Players:\n" + string.Join("\n", playerNames);
        gameNameText.text = gameManager.gameName.Value.ToString();
    }


    public void Disconnect()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            var connectedPlayersList = NetworkManager.Singleton.ConnectedClientsList;
            gameManager.playerNames.Remove(playerName);

            if (connectedPlayersList.Count <= 1)
            {
                gameManager.gameName.Value = "";
                gameManager.playerNames.Clear();
                gameManager.connectedPlayers.Value = 0;
                PrintInfoMessage("Host disconnected and game removed.");
            }
            else
            {
                PrintInfoMessage("Host disconnected.");
            }
            NetworkManager.Singleton.Shutdown();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            PrintInfoMessage("Client disconnected.");
            gameManager.playerNames.Remove(playerName);
        }
        UpdatePlayerListServerRpc();
    }

    public void PrintConnectedClients()
    {
        var connectedClients = NetworkManager.Singleton.ConnectedClientsList;
        if (connectedClients.Count == 0)
        {
            Debug.Log("No clients connected.");
            return;
        }
        Debug.Log("Connected Clients:");
        foreach (var client in connectedClients)
        {
            Debug.Log($"ClientId: {client.ClientId}");
        }
    }
}