using Unity.Netcode;
using UnityEngine;

public class ProgressTrackerOnNetwork : NetworkBehaviour
{
    [SerializeField] private ProgressTrackerSO _progressTracker;

    public float Progress { get {return _progressTracker.Progress;} }
    public float MaxProgress { get {return _progressTracker.MaxProgress;} }

    private NetworkVariable<float> _progress = new (value: 0);
    private NetworkVariable<float> _maxProgress = new (value: 0);

    public override void OnNetworkSpawn()
    {
        _progressTracker.TriggerProgressUpdate(_progress.Value);
        _progressTracker.SetMaxProgress(_maxProgress.Value);

        _progress.OnValueChanged += (float prev, float newValue) => _progressTracker.TriggerProgressUpdate(newValue);
        _maxProgress.OnValueChanged += (float prev, float newValue) => _progressTracker.SetMaxProgress(newValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetProgressServerRpc(float progress)
    {
        _progress.Value = progress;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMaxProgressServerRpc(float maxProgress)
    {
        _maxProgress.Value = maxProgress;
    }
}
