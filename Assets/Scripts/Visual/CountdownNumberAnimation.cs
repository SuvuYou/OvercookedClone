using TMPro;
using UnityEngine;

public class CountdownNumberAnimation : MonoBehaviour
{
    private const string COUNTDOWN_ANIMATION_TRIGGER = "Countdown";

    [SerializeField] private Animator _anim;
    [SerializeField] private TextMeshProUGUI _countdownNumber;

    public void SetNumberText(string newNumber)
    {
        if (_countdownNumber.text != newNumber)
        {
            _anim.SetTrigger(COUNTDOWN_ANIMATION_TRIGGER);
        }

        _countdownNumber.text = newNumber;
        
    }
}
