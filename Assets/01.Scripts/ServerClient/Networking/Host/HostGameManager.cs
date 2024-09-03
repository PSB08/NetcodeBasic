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

        var relayServerData = new RelayServerData(_allocation, "dtls"); //udp보다 보안이 향상된 버전
        transport.SetRelayServerData(relayServerData);

        try
        {
            //로비를 만들기 위한 옵션들을 넣는다.
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = false; //로비 옵션을 만들어서 넣어줘야 한다. 만약 이걸 true로 하면 조인코드로만 참석 가능

            //해당 로비 옵션에 Join코드를 넣어준다. (커스텀데이터를 이런식으로 넣는다)
            // Visbilty Member는 해당 로비의 멤버는 자유롭게 읽을 수 있다는 뜻.
            lobbyOptions.Data = new Dictionary<string, DataObject>()
            {
                {
                    "JoinCode", new DataObject(visibility: DataObject.VisibilityOptions.Member, value:_joinCode)
                }
            };
            //로비 이름과 옵션을 넣어주도록 되어 있음.
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync("Dummy Lobby", _maxConnections, lobbyOptions);


            //로비는 만들어진후 활동이 없으면 파괴되도록되어 있다. 따라서 일정시간간격으로 ping을 보내야 한다.
            _lobbyId = lobby.Id;
            HostSingleton.Instance.StartCoroutine(HeartBeatLobby(15)); //15초마다 핑
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
            Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId); //로비로 핑 보내고
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
            //유니티 네트워크 매니저 셧다운.
            NetworkManager.Singleton.Shutdown();
            return false;
        }
    }

}
