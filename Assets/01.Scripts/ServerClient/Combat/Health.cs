using System;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    public int maxHealth = 100;

    public event Action OnDieEvent;
    public event Action OnHealthChangedEvent;

    private bool _isDead;

    public override void OnNetworkSpawn()
    {
        Debug.Log(NetworkObject.OwnerClientId);
        if (IsClient)
        {
            currentHealth.OnValueChanged += HandleChangeHealthValue;
            HandleChangeHealthValue(0, maxHealth);
        }

        if (!IsServer) return;
        currentHealth.Value = maxHealth;
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
            currentHealth.OnValueChanged -= HandleChangeHealthValue;
    }

    public float GetNormalizedHealth()
    {
        return (float)currentHealth.Value / maxHealth;
    }

    private void HandleChangeHealthValue(int previousValue, int newValue)
    {
        OnHealthChangedEvent?.Invoke();
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if (_isDead) return;
        currentHealth.Value = Mathf.Clamp(currentHealth.Value + value, 0, maxHealth);
        if (currentHealth.Value == 0)
        {
            OnDieEvent?.Invoke();
            _isDead = true;
        }
    }




}
