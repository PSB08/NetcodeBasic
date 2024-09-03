using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("참조변수들")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _projectileSpawnTrm;
    [SerializeField] private GameObject _serverProjectilePrefab;
    [SerializeField] private GameObject _clientProjectilePrefab;

    [SerializeField] private Collider2D _playerCollider;

    [Header("세팅값들")]
    [SerializeField] private float _projectileSpeed;

    [SerializeField] private float _fireCooltime;

    public UnityEvent OnFire;

    private bool _shouldFire;
    private float _prevFireTime;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        _inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool button)
    {
        _shouldFire = button;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!_shouldFire) return;

        if (Time.time < _prevFireTime + _fireCooltime) return;

        PrimaryFireServerRpc(_projectileSpawnTrm.position, _projectileSpawnTrm.up);
        SpawnDummyProjectile(_projectileSpawnTrm.position, _projectileSpawnTrm.up);
        _prevFireTime = Time.time;
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 dir)
    {
        GameObject projectileInstance = Instantiate(_clientProjectilePrefab, spawnPos, Quaternion.identity);

        projectileInstance.transform.up = dir;
        Physics2D.IgnoreCollision(_playerCollider, projectileInstance.GetComponent<Collider2D>());

        OnFire?.Invoke();
        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigidBody))
        {
            rigidBody.velocity = rigidBody.transform.up * _projectileSpeed;
        }
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 dir)
    {
        GameObject projectileInstance = Instantiate(_serverProjectilePrefab, spawnPos, Quaternion.identity);

        projectileInstance.transform.up = dir;

        Physics2D.IgnoreCollision(_playerCollider, projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact damage))
        {
            damage.SetOwner(OwnerClientId);
        }

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigidBody))
        {
            rigidBody.velocity = rigidBody.transform.up * _projectileSpeed;
        }
        SpawnDummyProjectileClientRpc(spawnPos, dir);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 dir)
    {
        if (IsOwner) return;

        SpawnDummyProjectile(spawnPos, dir);
    }


}
