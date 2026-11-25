
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private Dictionary<ulong, NetworkPlayer> players = new();

    public IReadOnlyDictionary<ulong, NetworkPlayer> Players => players;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(NetworkPlayer player)
    {
        ulong clientId = player.OwnerClientId;

        if (players.ContainsKey(clientId))
            return;

        players.Add(clientId, player);

        Debug.Log($"PlayerManager : player {clientId} registered.");
    }

    public void UnregisterPlayer(ulong clientId)
    {
        if (!players.ContainsKey(clientId))
            return;

        players.Remove(clientId);

        Debug.Log($"PlayerManager : player {clientId} unregistered.");
    }

    public NetworkPlayer GetPlayer(ulong clientId)
    {
        players.TryGetValue(clientId, out NetworkPlayer player);
        return player;
    }
}
