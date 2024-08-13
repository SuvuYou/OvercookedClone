using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionBindingUI : ActionBindingDisplayUI
{
    [SerializeField] private Button _rebindButton;
    [SerializeField] private GameObject _rebindKeyOverlay;

    protected override void Start()
    {
        base.Start();

        _rebindKeyOverlay.SetActive(false);

        PlayerInput.Instance.OnRebindKeyStart += _enableKeyBindingOverlay;
        PlayerInput.Instance.OnRebindKeyComplete += _disableKeyBindingOverlay;

        _rebindButton.onClick.AddListener(() => PlayerInput.Instance.RebindKey(actionToBind: _action));
    }

    private void OnDestroy()
    {
        PlayerInput.Instance.OnRebindKeyStart -= _enableKeyBindingOverlay;
        PlayerInput.Instance.OnRebindKeyComplete -= _disableKeyBindingOverlay;
        _rebindButton.onClick.RemoveAllListeners();
    }


    private void _enableKeyBindingOverlay ()   
    {
        _rebindButton.enabled = false;
        _rebindKeyOverlay.SetActive(true);
    }

    private void _disableKeyBindingOverlay ()   
    {
        _rebindButton.enabled = true;
        _rebindKeyOverlay.SetActive(false);
    }
}


