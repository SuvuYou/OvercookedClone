using Unity.Netcode;
using UnityEngine;

public class ClientAuthoritativeNetworkTransform : NetworkBehaviour
{
    private Vector3 _targetPosition;

    private void Awake()
    {
        _targetPosition = transform.position;
    }

    private void Update()
    {
        transform.position = new Vector3(_targetPosition.x, transform.position.y, _targetPosition.z);
    }

    public void SetTargetPosition(Vector3 target)
    {
        _setTargetPositionLocally(target.x, target.z);
        _setTargetPositionServerRpc(target.x, target.z);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setTargetPositionServerRpc(float x, float z, ServerRpcParams rpcParams = default)
    {
        _setTargetPositionClientRpc(x, z, senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _setTargetPositionClientRpc(float x, float z, ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;

        _setTargetPositionLocally(x, z);
    }

    private void _setTargetPositionLocally(float x, float z)
    {
        _targetPosition.x = x;
        _targetPosition.z = z;
    }
}
