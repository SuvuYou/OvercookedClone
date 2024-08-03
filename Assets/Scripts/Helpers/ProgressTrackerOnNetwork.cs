using Unity.Netcode;
using UnityEngine;

public class ProgressTrackerOnNetwork : NetworkBehaviour
{
    [SerializeField] private ProgressTrackerSO _progressTracker;

    private bool _isContinousProgress = false;
    private float _continousProgressRate = 0;

    public float Progress { get {return _progressTracker.Progress;} }
    public float MaxProgress { get {return _progressTracker.MaxProgress;} }

    private NetworkVariable<float> _progress = new (value: 0);
    private NetworkVariable<float> _maxProgress = new (value: 0);

    public override void OnNetworkSpawn()
    {
        _setLocalProgress(_progress.Value);
        _setLocalMaxProgress(_maxProgress.Value);
    }

    private void Update()
    {
        if (_isContinousProgress)
        {
            _setLocalProgress(Progress + _continousProgressRate);
        }
    }

    public void EnableContiniousProgressUpdate(float progressRate)
    {
        if (_isContinousProgress) return;
        
        _enableLocalContiniousProgressUpdateServerRpc(progressRate);
        _enableLocalContiniousProgressUpdate(progressRate);
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

    private void _setLocalProgress(float progress)
    {
        _progressTracker.TriggerProgressUpdate(progress);
    }

    private void _setLocalMaxProgress(float maxProgress)
    {
        _progressTracker.SetMaxProgress(maxProgress);
    }

    public void _enableLocalContiniousProgressUpdate(float progressRate)
    {
        _isContinousProgress = true;
        _continousProgressRate = progressRate;
    }

    public void _disableLocalContiniousProgressUpdate()
    {
        _isContinousProgress = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setProgressServerRpc(float progress, ServerRpcParams rpcParams = default)
    {
        _progress.Value = progress;
        _setProgressClientRpc(progress, ignoreClientId: rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _setMaxProgressServerRpc(float maxProgress, ServerRpcParams rpcParams = default)
    {
        _maxProgress.Value = maxProgress;
        _setMaxProgressClientRpc(maxProgress, ignoreClientId: rpcParams.Receive.SenderClientId);
    }

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

    [ServerRpc(RequireOwnership = false)]
    public void _enableLocalContiniousProgressUpdateServerRpc(float progressRate)
    {
        _enableLocalContiniousProgressUpdateClientRpc(progressRate);
    }

    [ServerRpc(RequireOwnership = false)]
    public void _disableLocalContiniousProgressUpdateServerRpc()
    {
        _disableLocalContiniousProgressUpdateClientRpc();
    }

    [ClientRpc]
    public void _enableLocalContiniousProgressUpdateClientRpc(float progressRate)
    {
        _enableLocalContiniousProgressUpdate(progressRate);
    }

    [ClientRpc]
    public void _disableLocalContiniousProgressUpdateClientRpc()
    {
        _disableLocalContiniousProgressUpdate();
    }
}
