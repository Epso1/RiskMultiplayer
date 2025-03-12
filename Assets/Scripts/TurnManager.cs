
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
    [Header("Turn Management UI")]
    [SerializeField] Text turnText;

    [Header("Turn Variables")]
    public NetworkVariable<int> turnIndex = new NetworkVariable<int>
        (0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] int playerCount = 3;

    private void Awake()
    {
        turnIndex.OnValueChanged += (previousValue, newValue) => { UpdateTurnTextServerRpc(); };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadDivide)) IncrementIndexServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementIndexServerRpc()
    {
        turnIndex.Value = (turnIndex.Value + 1) % playerCount;
    }   

    [ServerRpc(RequireOwnership = false)]
    public void UpdateTurnTextServerRpc()
    {
        UpdateTurnTextClientRpc();
    }

    [ClientRpc]
    public void UpdateTurnTextClientRpc()
    {
        turnText.text = $"Turn:\n{turnIndex.Value}";
    }
        /// El siguiente bloque actualiza el texto de la lista de jugadores en todos los usuarios,
        /// independientemente de si se ejecuta desde un cliente o desde el host.
        /// Sirve como ejemplo de la estructura que debe seguirse para este comportamiento.


    //[ServerRpc(RequireOwnership = false)]
    //public void RequestUpdatePlayerIdsTextServerRpc()
    //{
    //    IReadOnlyList<ulong> connectedIds = NetworkManager.Singleton.ConnectedClientsIds;
    //    playerIds = connectedIds.ToList();
    //    playerIdsFixedString.Value = "Connected Players:\n" + string.Join("\n", playerIds);
    //    UpdatePlayerIdsTextClientRpc(playerIdsFixedString.Value.ToString());
    //    Debug.Log("PlayerIDs text updated: " + playerIdsFixedString.Value.ToString());
    //}

    //[ClientRpc]
    //private void UpdatePlayerIdsTextClientRpc(string updatedText)
    //{
    //    playerIdsText.text = updatedText;
    //}

}

