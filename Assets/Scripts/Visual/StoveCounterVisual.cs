using UnityEngine;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] StoveCounter _stove;
    [SerializeField] private GameObject _particles;
    [SerializeField] private GameObject _stoveOnVisual;

    private void Start()
    {
        _stove.OnStoveOn += _turnStoveVisualsOn;
        _stove.OnStoveOff += _turnStoveVisualsOff;
    }

    private void OnDestroy()
    {
        _stove.OnStoveOn -= _turnStoveVisualsOn;
        _stove.OnStoveOff -= _turnStoveVisualsOff;
    }

    private void _turnStoveVisualsOn()
    {
        _particles.SetActive(true);
        _stoveOnVisual.SetActive(true);
    }

    private void _turnStoveVisualsOff()
    {
        _particles.SetActive(false);
        _stoveOnVisual.SetActive(false);
    }

 
}
