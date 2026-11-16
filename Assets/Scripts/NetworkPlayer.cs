
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log(
            $"NetworkPlayer created | " +
            $"ClientId: {OwnerClientId} | " +
            $"IsOwner: {IsOwner}"
        );

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.RegisterPlayer(this);
        }
        else
        {
            Debug.LogError("PlayerManager not found!");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.UnregisterPlayer(OwnerClientId);
        }
    }
}
