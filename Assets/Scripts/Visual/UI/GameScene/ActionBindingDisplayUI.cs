using TMPro;
using UnityEngine;

public class ActionBindingDisplayUI : MonoBehaviour
{
    [SerializeField] protected PlayerInput.ActionBinding _action;
    [SerializeField] private TextMeshProUGUI _rebindButtonText;

    protected virtual void Start()
    {
        _updateActionKeyDisplay();

        PlayerInput.Instance.OnRebindKeyComplete += _updateActionKeyDisplay;
    }

    private void OnDestroy()
    {
        PlayerInput.Instance.OnRebindKeyComplete -= _updateActionKeyDisplay;
    }

    protected void _updateActionKeyDisplay()
    {
        string actionKey = PlayerInput.Instance.GetBindingKeyByAction(_action);

        if (actionKey.Length > 3) _rebindButtonText.text = actionKey.Substring(0, 3);
        else _rebindButtonText.text = actionKey;
    }
}
