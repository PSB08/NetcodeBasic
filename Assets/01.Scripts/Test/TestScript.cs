using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;

    private void Awake()
    {
        _inputReader.MovementEvent += HandleMovement;
    }

    private void OnDestroy()
    {
        _inputReader.MovementEvent -= HandleMovement;
    }

    private void HandleMovement(Vector2 move)
    {
        Debug.Log(move);
    }

}
