using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class CoinSpawner : NetworkBehaviour
{
    [Header("���� ��")]
    [SerializeField] private RespawningCoins _coinPrefab;
    [SerializeField] private DecalCircle _decalCircle;


    [Header("���ð�")]
    [SerializeField] private int _maxCoins = 30;
    [SerializeField] private int _coinValue = 10; 
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _spawningTerm = 30f;
    [SerializeField] private float _spawnRadius = 8f;
    private bool _isSpawning = false; 
    private float _spawnTime = 0;
    private int _spawnCountTime = 10; 


    public List<Transform> spawnPointList; 
    private float _coinRadius;

    private Stack<RespawningCoins> _coinPool = new Stack<RespawningCoins>(); 
    private List<RespawningCoins> _activeCoinList = new List<RespawningCoins>(); 

    private RespawningCoins SpawnCoin()
    {
        RespawningCoins coinInstance = Instantiate(_coinPrefab, Vector3.zero, Quaternion.identity);
        coinInstance.SetValue(_coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn(); 
        coinInstance.OnCollected += HandleCoinCollected;

        return coinInstance;
    }


    private void HandleCoinCollected(RespawningCoins coin)
    {
        _activeCoinList.Remove(coin); 
        coin.SetVisible(false);
        _coinPool.Push(coin);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }


        _coinRadius = _coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < _maxCoins; i++)
        {
            var coin = SpawnCoin();
            coin.SetVisible(false); 
            _coinPool.Push(coin);
        }
    }

    public override void OnNetworkDespawn()
    {
        StopAllCoroutines();
    }


    private void Update()
    {
        if (!IsServer) return; 

        if (!_isSpawning && _activeCoinList.Count == 0)
        {
            _spawnTime += Time.deltaTime;
            if (_spawnTime >= _spawningTerm)
            {
                _spawnTime = 0;
                StartCoroutine(SpawnCoroutine());
            }
        }
    }

    [ClientRpc]
    private void ServerCountDownMessageClientRpc(int sec, int pointIdx, int coinCount)
    {
        if (!_decalCircle.showDecal)
        {
            _decalCircle.OpenCircle(spawnPointList[pointIdx].position, _spawnRadius);
        }

        Debug.Log($"{pointIdx} �� �������� {sec}���� {coinCount}���� ������ �����˴ϴ�.");
    }

    [ClientRpc]
    private void DecalCircleClientRpc()
    {
        _decalCircle.CloseCircle();
    }

    IEnumerator SpawnCoroutine()
    {
        _isSpawning = true;
        int pointIdx = Random.Range(0, spawnPointList.Count);
        int coinCount = Random.Range(_maxCoins / 2, _maxCoins + 1);

        for (int i = _spawnCountTime; i > 0; i--)
        {
            ServerCountDownMessageClientRpc(i, pointIdx, coinCount);
            yield return new WaitForSeconds(1f);
        }

        Vector2 center = spawnPointList[pointIdx].position;
        for (int i = 0; i < coinCount; ++i)
        {
            Vector2 pos = Random.insideUnitCircle * _spawnRadius + center;
            var coin = _coinPool.Pop();
            coin.transform.position = pos;
            coin.ResetCoin();
            _activeCoinList.Add(coin);
            yield return new WaitForSeconds(4f); 
        }
        _isSpawning = false;
        DecalCircleClientRpc(); 
    }
}
