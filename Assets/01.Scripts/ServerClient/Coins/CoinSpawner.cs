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
    [SerializeField] private int _maxCoins = 30; //30���� ����Ǯ ����
    [SerializeField] private int _coinValue = 10; //���δ� 10
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _spawningTerm = 30f;
    [SerializeField] private float _spawnRadius = 8f;
    private bool _isSpawning = false; //������ ���ΰ�?
    private float _spawnTime = 0;
    private int _spawnCountTime = 10; //10��ī�����ϰ� ����


    public List<Transform> spawnPointList; //������ �������� ����Ʈ
    private float _coinRadius;

    private Stack<RespawningCoins> _coinPool = new Stack<RespawningCoins>(); //����Ǯ
    private List<RespawningCoins> _activeCoinList = new List<RespawningCoins>(); //Ȱ��ȭ�� ���ε�

    private RespawningCoins SpawnCoin()
    {
        RespawningCoins coinInstance = Instantiate(_coinPrefab, Vector3.zero, Quaternion.identity);
        coinInstance.SetValue(_coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn(); //������ Ŭ��鿡�� ������ �˸�
        coinInstance.OnCollected += HandleCoinCollected;

        return coinInstance;
    }

    //�̰� ������ �����Ѵ�. Ŭ�󿡼� ���ߴ°� Ŭ�� �˾Ƽ� �ҰŴ�.
    private void HandleCoinCollected(RespawningCoins coin)
    {
        //�ݷ�Ʈ�� ������ ����Ʈ�� �ٽ� �ִ´�
        _activeCoinList.Remove(coin); //����Ʈ���� ���� �����ϰ� 
        coin.SetVisible(false);
        _coinPool.Push(coin);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }


        //ó�� �����ϸ� ������ �ִ� ���θ�ŭ ������ ���Ѽ� Ǯ���Ѵ�.
        _coinRadius = _coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < _maxCoins; i++)
        {
            var coin = SpawnCoin();
            coin.SetVisible(false); //ó�� ������ �ֵ��� ���ش�.
            _coinPool.Push(coin);
        }
    }

    public override void OnNetworkDespawn()
    {
        StopAllCoroutines(); //�ڷ�ƾ ��� ����
    }


    private void Update()
    {
        if (!IsServer) return; //���� �ƴϰ�� ī��Ʈ �ʿ� ����.

        //���߿� ���⼭ ������ ���ӽ��ۿ� ���� ���� �����ϵ��� �� �ʿ䰡 ����.

        //Ȱ��ȭ�� ������ ���� ���������� �ƴ϶�� �ð��� ���ϱ� ����
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
        //�̰� ������ �����ϴϱ� ���� �Ȱɷ��� ��
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
            yield return new WaitForSeconds(4f); //4�ʸ��� ���� �ϳ���
        }
        _isSpawning = false;
        DecalCircleClientRpc(); //Ŭ�󿡼� �ݾ���
    }
}
