using System.Collections.Generic;
using UnityEngine;

public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] private PlateCounter _platesCounter;
    [SerializeField] private GameObject _plateVisualPrefab;
    [SerializeField] private Transform _plateSpawnPlaceholder;

    private const float _singlePlateYOffset = 0.1f;

    private Stack<GameObject> _platesStack = new();

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
        if (newPlatesCount < _platesStack.Count) 
        {
            _removePlatesFromStack(numberOfPlates: _platesStack.Count - newPlatesCount);
        }
        else if (newPlatesCount > _platesStack.Count)
        {
            _addPlatesToStack(numberOfPlates: newPlatesCount - _platesStack.Count);
        } 
    }

    private void _removePlatesFromStack(int numberOfPlates)
    {
        for (int i = 0; i < numberOfPlates; i++)
        {
            Destroy(_platesStack.Pop().gameObject);
        }
    }

    private void _addPlatesToStack(int numberOfPlates)
    {
        for (int i = 0; i < numberOfPlates; i++)
        {
            Vector3 offset = new (0f, _platesStack.Count * _singlePlateYOffset, 0f);
            GameObject plate = Instantiate(_plateVisualPrefab, _plateSpawnPlaceholder.position + offset, _plateVisualPrefab.transform.rotation);
            plate.transform.parent = _plateSpawnPlaceholder;
            _platesStack.Push(plate); 
        }
    }

}
