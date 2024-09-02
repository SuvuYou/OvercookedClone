using Unity.Netcode;
using UnityEngine;

public class ProgressTrackerOnNetwork : NetworkBehaviour
{
    [SerializeField] private ProgressTracker _progressTracker = new();
    public ProgressTracker ProgressTracker { get => _progressTracker; }

    private bool _isContinousProgress = false;

    public float Progress { get => _progressTracker.Progress; }
    public float MaxProgress { get => _progressTracker.MaxProgress; }

    private void Update()
    {
        if (_isContinousProgress)
        {
            _setLocalProgress(Progress + Time.deltaTime);
        }
    }

    public void EnableContiniousProgressUpdate()
    {
        if (_isContinousProgress) return;
        
        _enableLocalContiniousProgressUpdateServerRpc();
        _enableLocalContiniousProgressUpdate();
    }

    public void DisableContiniousProgressUpdate()
    {
        if (!_isContinousProgress) return;

        _disableLocalContiniousProgressUpdateServerRpc();
        _disableLocalContiniousProgressUpdate();
    }

    public void SetProgress(float progress)
    {
        _setLocalProgress(progress);
        _setProgressServerRpc(progress);
    }

    public void SetMaxProgress(float progress)
    {
        _setLocalMaxProgress(progress);
        _setMaxProgressServerRpc(progress);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void _setProgressServerRpc(float progress, ServerRpcParams rpcParams = default) => _setProgressClientRpc(progress, ignoreClientId: rpcParams.Receive.SenderClientId);
    
    [ServerRpc(RequireOwnership = false)]
    private void _setMaxProgressServerRpc(float maxProgress, ServerRpcParams rpcParams = default) => _setMaxProgressClientRpc(maxProgress, ignoreClientId: rpcParams.Receive.SenderClientId);
    
    [ClientRpc]
    private void _setProgressClientRpc(float progress, ulong ignoreClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == ignoreClientId) return;

        _setLocalProgress(progress);
    }

    [ClientRpc]
    private void _setMaxProgressClientRpc(float maxProgress, ulong ignoreClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == ignoreClientId) return;

        _setLocalMaxProgress(maxProgress);
    }

    private void _setLocalProgress(float progress) => _progressTracker.TriggerProgressUpdate(progress);
    
    private void _setLocalMaxProgress(float maxProgress) => _progressTracker.SetMaxProgress(maxProgress);

    [ServerRpc(RequireOwnership = false)]
    public void _enableLocalContiniousProgressUpdateServerRpc() => _enableLocalContiniousProgressUpdateClientRpc();
    
    [ClientRpc]
    public void _enableLocalContiniousProgressUpdateClientRpc() => _enableLocalContiniousProgressUpdate();

    public void _enableLocalContiniousProgressUpdate() => _isContinousProgress = true;

    [ServerRpc(RequireOwnership = false)]
    public void _disableLocalContiniousProgressUpdateServerRpc() => _disableLocalContiniousProgressUpdateClientRpc();
    
    [ClientRpc]
    public void _disableLocalContiniousProgressUpdateClientRpc() => _disableLocalContiniousProgressUpdate();

    public void _disableLocalContiniousProgressUpdate() => _isContinousProgress = false;
}
