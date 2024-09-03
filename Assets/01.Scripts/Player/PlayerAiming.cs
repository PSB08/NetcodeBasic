using Unity.Netcode;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _turrentTransform;

    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector2 mousePos = _inputReader.AimPosition;

        Vector3 worldMousePos = CameraManager.Instance.MainCam.ScreenToWorldPoint(mousePos);

        Vector3 dir = (worldMousePos - transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        _turrentTransform.rotation = Quaternion.Euler(0, 0, angle);


    }

}
