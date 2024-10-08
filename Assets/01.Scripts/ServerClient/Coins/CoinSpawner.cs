using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class CoinSpawner : NetworkBehaviour
{
    [Header("참조 값")]
    [SerializeField] private RespawningCoins _coinPrefab;
    [SerializeField] private DecalCircle _decalCircle;


    [Header("셋팅값")]
    [SerializeField] private int _maxCoins = 30; //30개의 코인풀 유지
    [SerializeField] private int _coinValue = 10; //코인당 10
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _spawningTerm = 30f;
    [SerializeField] private float _spawnRadius = 8f;
    private bool _isSpawning = false; //스포닝 중인가?
    private float _spawnTime = 0;
    private int _spawnCountTime = 10; //10초카운팅하고 시작


    public List<Transform> spawnPointList; //코인을 스포닝할 리스트
    private float _coinRadius;

    private Stack<RespawningCoins> _coinPool = new Stack<RespawningCoins>(); //코인풀
    private List<RespawningCoins> _activeCoinList = new List<RespawningCoins>(); //활성화된 코인들

    private RespawningCoins SpawnCoin()
    {
        RespawningCoins coinInstance = Instantiate(_coinPrefab, Vector3.zero, Quaternion.identity);
        coinInstance.SetValue(_coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn(); //서버가 클라들에게 스폰을 알림
        coinInstance.OnCollected += HandleCoinCollected;

        return coinInstance;
    }

    //이건 서버만 실행한다. 클라에서 감추는건 클라가 알아서 할거다.
    private void HandleCoinCollected(RespawningCoins coin)
    {
        //콜렉트된 코인은 리스트로 다시 넣는다
        _activeCoinList.Remove(coin); //리스트에선 코인 삭제하고 
        coin.SetVisible(false);
        _coinPool.Push(coin);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }


        //처음 시작하면 서버만 최대 코인만큼 스포닝 시켜서 풀링한다.
        _coinRadius = _coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < _maxCoins; i++)
        {
            var coin = SpawnCoin();
            coin.SetVisible(false); //처음 생성된 애들은 꺼준다.
            _coinPool.Push(coin);
        }
    }

    public override void OnNetworkDespawn()
    {
        StopAllCoroutines(); //코루틴 모두 정지
    }


    private void Update()
    {
        if (!IsServer) return; //서버 아니고는 카운트 필요 없다.

        //나중에 여기서 서버가 게임시작에 들어갔을 때만 진행하도록 할 필요가 있음.

        //활성화된 코인이 없고 스포닝중이 아니라면 시간을 더하기 시작
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

        Debug.Log($"{pointIdx} 번 지점에서 {sec}초후 {coinCount}개의 코인이 생성됩니다.");
    }

    [ClientRpc]
    private void DecalCircleClientRpc()
    {
        _decalCircle.CloseCircle();
    }

    IEnumerator SpawnCoroutine()
    {
        //이건 서버만 실행하니까 굳이 안걸러도 돼
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
            yield return new WaitForSeconds(4f); //4초마다 코인 하나씩
        }
        _isSpawning = false;
        DecalCircleClientRpc(); //클라에서 닫아줘
    }
}
