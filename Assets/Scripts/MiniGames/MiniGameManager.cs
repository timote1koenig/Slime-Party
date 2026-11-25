
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameManager : NetworkBehaviour
{
    [Header("Character")]
    [SerializeField] private GameObject characterPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private readonly List<NetworkObject> spawnedCharacters = new();

    private bool hasSpawned = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        Debug.Log("MiniGameManager : waiting for client loading.");

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted
            += HandleLoadEventCompleted;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted
                -= HandleLoadEventCompleted;
        }
    }


    private void Update()
    {
        if (!IsServer)
            return;

        if (UnityEngine.InputSystem.Keyboard.current == null)
            return;

        if (UnityEngine.InputSystem.Keyboard.current.bKey.wasPressedThisFrame)
        {
            EndMiniGame();
        }
    }

    private void EndMiniGame()
    {
        if (!IsServer)
            return;

        Debug.Log("End of mini-game !");

        DespawnPlayers();

        NetworkManager.Singleton.SceneManager.LoadScene(
            "MultiplayerScene",
            LoadSceneMode.Single
        );
    }


    private void HandleLoadEventCompleted(
        string sceneName,
        LoadSceneMode loadSceneMode,
        List<ulong> clientsCompleted,
        List<ulong> clientsTimedOut)
    {
        if (!IsServer)
            return;

        if (sceneName != gameObject.scene.name)
            return;

        if (hasSpawned)
            return;

        Debug.Log(
            $"Load completed : {sceneName} | " +
            $"Synchronized clients : {clientsCompleted.Count} | " +
            $"Timeouts : {clientsTimedOut.Count}"
        );

        if (clientsTimedOut.Count > 0)
        {
            Debug.LogError("Some clients have not completed the loading.");
            return;
        }

        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        if (characterPrefab == null)
        {
            Debug.LogError("Character Prefab not assigned !");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points configured !");
            return;
        }

        var connectedClients =
            NetworkManager.Singleton.ConnectedClientsIds;

        int index = 0;

        foreach (ulong clientId in connectedClients)
        {
            var client =
                NetworkManager.Singleton.ConnectedClients[clientId];

            if (client.PlayerObject == null)
            {
                Debug.LogError(
                    $"PlayerObject not found for {clientId}"
                );
                continue;
            }

            Transform spawnPoint =
                spawnPoints[index % spawnPoints.Length];

            GameObject character = Instantiate(
                characterPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            NetworkObject networkObject =
                character.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError("NetworkObject not found !");
                Destroy(character);
                continue;
            }

            networkObject.SpawnWithOwnership(clientId, true);

            spawnedCharacters.Add(networkObject);

            Debug.Log(
                $"Character created for player {clientId} " +
                $"| NetworkObjectId : {networkObject.NetworkObjectId}"
            );

            index++;
        }

        hasSpawned = true;
    }

    public void DespawnPlayers()
    {
        if (!IsServer)
            return;

        foreach (NetworkObject character in spawnedCharacters)
        {
            if (character != null && character.IsSpawned)
                character.Despawn(true);
        }

        spawnedCharacters.Clear();
        hasSpawned = false;
    }
}
