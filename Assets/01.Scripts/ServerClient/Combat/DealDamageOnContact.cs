using Unity.Netcode;
using UnityEngine;

public class DealDamageOnContact : MonoBehaviour
{
    [SerializeField] private int _damage = 10;

    private ulong _ownerClientID;

    public void SetOwner(ulong ownerClientID)
    {
        _ownerClientID = ownerClientID;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody is null) return;

        if (other.attachedRigidbody.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            if (netObj.OwnerClientId == _ownerClientID) return;
        }

        if (other.attachedRigidbody.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(_damage);
        }
    }

}
