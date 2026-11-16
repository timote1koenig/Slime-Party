
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayConnectionManager : MonoBehaviour
{
    private string joinCode = "";
    private string status = "Waiting...";

    private bool isConnecting = false;

    private async Task InitializeServices()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void StartHostWithRelay()
    {
        if (isConnecting || NetworkManager.Singleton.IsListening)
            return;

        isConnecting = true;
        status = "Creating room...";

        try
        {
            await InitializeServices();

            Allocation allocation =
                await RelayService.Instance.CreateAllocationAsync(1);

            string code =
                await RelayService.Instance.GetJoinCodeAsync(
                    allocation.AllocationId
                );

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, "dtls")
            );

            if (NetworkManager.Singleton.StartHost())
            {
                joinCode = code;
                status = "Room created!";
                Debug.Log("Relay Code : " + joinCode);
            }
            else
            {
                status = "Failed to start the host.";
            }
        }
        catch (Exception e)
        {
            status = "Error : " + e.Message;
            Debug.LogException(e);
        }
        finally
        {
            isConnecting = false;
        }
    }

    public async void StartClientWithRelay()
    {
        if (isConnecting || NetworkManager.Singleton.IsListening)
            return;

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            status = "Enter a Relay code.";
            return;
        }

        isConnecting = true;
        status = "Connecting to the room...";

        try
        {
            await InitializeServices();

            JoinAllocation allocation =
                await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            transport.SetRelayServerData(
                AllocationUtils.ToRelayServerData(allocation, "dtls")
            );

            if (NetworkManager.Singleton.StartClient())
            {
                status = "Connection in progress...";
            }
            else
            {
                status = "Failed to start the client.";
            }
        }
        catch (Exception e)
        {
            status = "Error : " + e.Message;
            Debug.LogException(e);
        }
        finally
        {
            isConnecting = false;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 180));

        GUILayout.Label("RELAY MULTIPLAYER");
        GUILayout.Label("Status : " + status);

        GUILayout.Label("Join Code :");
        joinCode = GUILayout.TextField(joinCode, 20);

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsHost)
        {
            GUILayout.Label("Your Code : " + joinCode);
        }

        GUILayout.EndArea();
    }
}
