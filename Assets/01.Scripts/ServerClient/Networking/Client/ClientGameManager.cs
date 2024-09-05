using System;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;



public class ClientGameManager
{
    private JoinAllocation _allocation;

    public async Task<bool> InitAsync()
    {

        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile($"UUID_{UnityEngine.Random.Range(100000000, 999999999)}");
        await UnityServices.InitializeAsync(initializationOptions);
        Debug.Log(AuthenticationService.Instance.Profile);

        AuthState authState = await UGSAuthWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            return true;
        }
        return false;
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene(SceneNames.MenuScene);
    }

    public async Task StartClientWithJoinCode(string joinCode)
    {
        try
        {
            _allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var relayServerData = new RelayServerData(_allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();


        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }
    }

    public bool StartClientWithLocal()
    {
        if (!NetworkManager.Singleton.StartClient())
        {
            NetworkManager.Singleton.Shutdown();
            return false;
        }
        return true;
    }


}
