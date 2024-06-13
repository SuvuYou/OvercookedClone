using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectCharacterUI : MonoBehaviour
{
    [SerializeField] private Button _readyButton;

    private void Start()
    {
        _readyButton.onClick.AddListener(() => PlayerReadyManager.Instance.SetLocalPlayerReady());
    }
}
