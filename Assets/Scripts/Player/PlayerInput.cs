using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance;

    private const string ACTION_BINDINGS_PLAYER_PREF = "ActionBindings";

    public enum ActionBinding
    {
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft,
        MoveJoyStick,
        Interact,
        InteractGamepad,
        InteractAlt,
        InteractAltGamepad,
        Pause,
        PauseGamepad
    }

    public event Action OnInteractDuringWaitingState;
    public event Action OnInteractDuringEditing;

    public event Action OnInteract;
    public event Action OnInteractAlternative;
    public event Action OnPausePressed;

    public event Action OnRebindKeyStart;
    public event Action OnRebindKeyComplete;

    private PlayerInputActions _inputSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);

            return;
        }

        Instance = this;

        _inputSystem = new PlayerInputActions();

        if (PlayerPrefs.HasKey(ACTION_BINDINGS_PLAYER_PREF)) _inputSystem.LoadBindingOverridesFromJson(PlayerPrefs.GetString(ACTION_BINDINGS_PLAYER_PREF));

        _inputSystem.Standard.Enable();
        _inputSystem.Standard.Interact.performed += _interactAction;
        _inputSystem.Standard.InteractAlternative.performed += _interactAlternativeAction;
        _inputSystem.Standard.Pause.performed += _pausePressed;
    }

    private void OnDestroy()
    {
        _inputSystem.Standard.Interact.performed -= _interactAction;
        _inputSystem.Standard.InteractAlternative.performed -= _interactAlternativeAction;
        _inputSystem.Standard.Pause.performed -= _pausePressed;

        _inputSystem.Dispose();
    }

    public Vector3 GetMovementDirectionVector()
    {
        Vector2 inputVector = _inputSystem.Standard.Move.ReadValue<Vector2>();

        Vector3 movementDirection = new(inputVector.x, 0f, inputVector.y);

        return movementDirection.normalized;
    }

    private void _pausePressed(InputAction.CallbackContext context)
    {
        OnPausePressed?.Invoke();
    }

    private void _interactAction(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.IsPaused) return;

        switch (GameManager.Instance.State)
        {
            case GameState.Waiting:
                OnInteractDuringWaitingState?.Invoke();
                break;
            case GameState.Active:
                OnInteract?.Invoke();
                break;
            case GameState.Editing:
                OnInteractDuringEditing?.Invoke();
                break;
        }
    }

    private void _interactAlternativeAction(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.IsPaused) return;

        if (GameManager.Instance.State != GameState.Active) return;

        OnInteractAlternative?.Invoke();
    }

    public string GetBindingKeyByAction(ActionBinding action)
    {
        return action switch
        {
            ActionBinding.MoveUp => _inputSystem.Standard.Move.bindings[1].ToDisplayString(),
            ActionBinding.MoveRight => _inputSystem.Standard.Move.bindings[2].ToDisplayString(),
            ActionBinding.MoveDown => _inputSystem.Standard.Move.bindings[3].ToDisplayString(),
            ActionBinding.MoveLeft => _inputSystem.Standard.Move.bindings[4].ToDisplayString(),
            ActionBinding.MoveJoyStick => _inputSystem.Standard.Move.bindings[5].ToDisplayString(),
            ActionBinding.Pause => _inputSystem.Standard.Pause.bindings[0].ToDisplayString(),
            ActionBinding.PauseGamepad => _inputSystem.Standard.Pause.bindings[1].ToDisplayString(),
            ActionBinding.Interact => _inputSystem.Standard.Interact.bindings[0].ToDisplayString(),
            ActionBinding.InteractGamepad => _inputSystem.Standard.Interact.bindings[1].ToDisplayString(),
            ActionBinding.InteractAlt => _inputSystem.Standard.InteractAlternative.bindings[0].ToDisplayString(),
            ActionBinding.InteractAltGamepad => _inputSystem.Standard.InteractAlternative.bindings[1].ToDisplayString(),
            _ => "ERR"
        };
    }

    public void RebindKey(ActionBinding actionToBind)
    {
        OnRebindKeyStart?.Invoke();

        switch (actionToBind)
        {
            case ActionBinding.MoveUp:
                _performRebind(_inputSystem.Standard.Move, actionIndex: 1);
                break;
            case ActionBinding.MoveRight:
                _performRebind(_inputSystem.Standard.Move, actionIndex: 2);
                break;
            case ActionBinding.MoveDown:
                _performRebind(_inputSystem.Standard.Move, actionIndex: 3);
                break;
            case ActionBinding.MoveLeft:
                _performRebind(_inputSystem.Standard.Move, actionIndex: 4);
                break;
            case ActionBinding.Pause:
                _performRebind(_inputSystem.Standard.Pause, actionIndex: 0);
                break;
            case ActionBinding.Interact:
                _performRebind(_inputSystem.Standard.Interact, actionIndex: 0);
                break;
            case ActionBinding.InteractAlt:
                _performRebind(_inputSystem.Standard.InteractAlternative, actionIndex: 0);
                break;
            default:
                OnRebindKeyComplete?.Invoke();
                break;
        }
    }

    private void _performRebind(InputAction action, int actionIndex)
    {
        _inputSystem.Standard.Disable();

        action
        .PerformInteractiveRebinding(actionIndex)
        .OnComplete(cb =>
        {
            OnRebindKeyComplete?.Invoke();
            _inputSystem.Standard.Enable();
            PlayerPrefs.SetString(ACTION_BINDINGS_PLAYER_PREF, _inputSystem.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        })
        .Start();
    }
}
