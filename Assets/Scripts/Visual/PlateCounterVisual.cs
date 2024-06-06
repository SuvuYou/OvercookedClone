using System.Collections.Generic;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private PlateCounter _platesCounter;
    [SerializeField] private GameObject _plateVisualPrefab;
    [SerializeField] private Transform _plateSpawnPlaceholder;

    private const float _singlePlateYOffset = 0.1f;

    private Stack<GameObject> _plates = new();

    private void Start()
    {
        _platesCounter.OnPlateNumberChange += _updatePlatesCount;
    }

    private void OnDestroy()
    {
        _platesCounter.OnPlateNumberChange -= _updatePlatesCount;
    }

    private void _updatePlatesCount (int newPlatesCount)
    {
        if (newPlatesCount < _plates.Count) 
        {
            Destroy(_plates.Pop().gameObject);
        }
        else if (newPlatesCount > _plates.Count)
        {
            Vector3 offset = new (0f, _plates.Count * _singlePlateYOffset, 0f);
            GameObject plate = Instantiate(_plateVisualPrefab, _plateSpawnPlaceholder.position + offset, _plateVisualPrefab.transform.rotation);
            plate.transform.parent = _plateSpawnPlaceholder;
            _plates.Push(plate); 
        } 
    }

}
