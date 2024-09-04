using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class IPConnectUI : MonoBehaviour 
{
    [SerializeField] private Button _hostBtn, _clientBtn;
    [SerializeField] private TMP_InputField _ipText, _portText;

    private void Awake()
    {
        if (NetworkManager.Singleton == null) return;

        _hostBtn.onClick.AddListener(HandleHostBtnClick);
        _clientBtn.onClick.AddListener(HandleClientBtnClick);

        _ipText.text = FindIPAddress();
        _portText.text = "7777";
        
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

    }

    private string FindIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                //Debug.Log(ip);
                return ip.ToString();
            }
        }
        return null;
    }

    private void HandleHostBtnClick()
    {
        if (!SetUpNetworkPassport()) return;

        bool result = HostSingleton.Instance.GameManager.StartHostLocalNetwork();

    }

    private void HandleClientBtnClick()
    {
        if (!SetUpNetworkPassport()) return;

        bool result = ClientSingletone.Instance.GameManager.StartClientWithLocal();
    }

    //�Է��� ip�� ��Ʈ��ȣ�� ������ �˻� �� �ùٸ��� ��Ʈ��Ʈ ����
    private bool SetUpNetworkPassport()
    {
        var ip = _ipText.text;
        var port = _portText.text;

        var portRegex = new Regex(@"[0-9]{3,5}");
        var ipRegex = new Regex(@"^[0-9\.]+$");

        var portMatch = portRegex.Match(port);
        var ipMatch = ipRegex.Match(ip);

        if (!portMatch.Success || !ipMatch.Success)
        {
            Debug.LogError("���� ���� ip�� ��Ʈ��ȣ �Դϴ�");
            return false;
        }

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip, (ushort)int.Parse(port));
        return true;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        Debug.Log(clientID + "�����߻�");
    }


}
