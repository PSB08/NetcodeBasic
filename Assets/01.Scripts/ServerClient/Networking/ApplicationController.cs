using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingletone _clientPrefab;
    [SerializeField] private HostSingleton _hostPrefab;


    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);
    }

    private async Task LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            //do
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(_hostPrefab);
            hostSingleton.CreateHost();

            ClientSingletone clientSingletone = Instantiate(_clientPrefab);
            bool authenticated = await clientSingletone.CreateClient();

            if (authenticated)
            {
                Debug.Log("Load");
                ClientSingletone.Instance.GameManager.GotoMenu();
            }
            else
            {
                Debug.LogError("UGS Serivce login failed");
            }

        }

    }


}
