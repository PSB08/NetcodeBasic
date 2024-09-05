using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;




public class HostGameManager : MonoBehaviour
{
    private const string GameScene = "GameScene";
    private Allocation _allocation;
    private string _joinCode;
    private string _lobbyId;
    private const int _maxConnections = 8;
    public async Task StartHostAsync()
    {
        try
        {
            _allocation = await Relay.Instance.CreateAllocationAsync(_maxConnections);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        try
        {
            _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
            Debug.Log(_joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        var relayServerData = new RelayServerData(_allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        try
        {
            
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false; 

          
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value:_joinCode)
                }
            };
           
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Dummy Lobby", _maxConnections, lobbyOptions);


            _lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15)); 
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
            return;
        }


        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(SceneNames.MainScene, LoadSceneMode.Single);

    }

    private IEnumerator HeartBeatLobby(float waitTimeSec)
    {
        var timer = new WaitForSecondsRealtime(waitTimeSec);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId);
            yield return timer;
        }
    }


    public bool StartHostLocalNetwork()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneNames.MainScene, LoadSceneMode.Single);
            return true;
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            return false;
        }
    }

}
