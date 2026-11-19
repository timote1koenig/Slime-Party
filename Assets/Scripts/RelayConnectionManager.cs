using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayConnectionManager : MonoBehaviour
{
    private string joinCode = "";
    private string status = "";
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private RelayUI relay;

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

        relay.HostSelected();

        isConnecting = true;
        status = "Creating room...";
        relay.SetStatus(status);

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
                relay.SetHostCode(joinCode);
                status = "Room created!";
                relay.SetStatus(status);
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

        relay.JoinSelected();

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            status = "Enter a Relay code.";
            relay.SetStatus(status);
            return;
        }

        isConnecting = true;
        status = "Connecting to the room...";
        relay.SetStatus(status);

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
                relay.SetStatus(status);
            }
            else
            {
                status = "Failed to start the client.";
                relay.SetStatus(status);
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
            status = "Connected.";
            relay.JoinSuccess();
            relay.SetStatus(status);
        }
    }
    public void OnHostButtonClicked()
    {
        StartHostWithRelay();
    }

    public void OnJoinButtonClicked()
    {
        joinCode = joinCodeInput.text;
        StartClientWithRelay();
    }
}
