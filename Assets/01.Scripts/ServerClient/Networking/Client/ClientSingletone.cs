using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ClientSingletone : MonoBehaviour
{
    private static ClientSingletone instance;

    public ClientGameManager GameManager { get; set; }
    public static ClientSingletone Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<ClientSingletone>();

            if (instance == null)
            {
                Debug.LogError("Client Singleton 없다 비사ㅏㅇ");
                return null;
            }

            return instance;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();

        return await GameManager.InitAsync();
    }

}
