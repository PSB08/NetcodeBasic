using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;




public class HostGameManager : MonoBehaviour
{
    private const string GameScene = "GameScene";
    private Allocation _allocation;
    private string _joinCode;
    private const int _maxConnections = 20;
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

        var relayServerData = new RelayServerData(_allocation, "dtls"); //udp���� ������ ���� ����
        transport.SetRelayServerData(relayServerData);

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(SceneNames.MainScene, LoadSceneMode.Single);
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
            //����Ƽ ��Ʈ��ũ �Ŵ��� �˴ٿ�.
            NetworkManager.Singleton.Shutdown();
            return false;
        }
    }

}
