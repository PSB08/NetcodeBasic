using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawningCoins : Coin
{
    public event Action<RespawningCoins> OnCollected;
    private Vector2 _prevPos;

    public override int Collect()
    {
        if (_alreadyCollected) return 0;

        if (!IsServer)
        {
            SetVisible(false);
            return 0;
        }

        _alreadyCollected = true;
        OnCollected?.Invoke(this);
        isActive.Value = false;

        return _coinValue;

    }


    [ContextMenu("resetCoin")]
    public void ResetCoin()
    {
        _alreadyCollected = false;
        isActive.Value = true;
        SetVisible(true);


    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _prevPos = transform.position;

    }


    private void Update()
    {
        if (IsServer) return;

        if (Vector2.Distance(_prevPos, transform.position) >= 0.1f)
        {
            _prevPos = transform.position;
            SetVisible(true);
        }
    }


}
