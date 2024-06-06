using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance;

    public event Action OnAddRecipe;
    public event Action OnRemoveRecipe;
    public event Action OnDeliverySuccess;
    public event Action OnDeliveryFailed;

    [SerializeField] private AvailableRecipesListSO _availableRecipesList;
    private List<RecipeSO> _currentWaitingRecipeList = new();

    private bool _isAddingNewRecipe = false;

    private const int _maxRecipeWaitingCount = 4; 

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Active)
        {
            return;
        }

        _addRecipeToWaitingList();
    }

    private void _addRecipeToWaitingList()
    {
        if (!_isAddingNewRecipe)
        {
            StartCoroutine(_addRecipeToWaitingListCoroutine());
        }
    } 

    private IEnumerator _addRecipeToWaitingListCoroutine()
    {
        _isAddingNewRecipe = true;

        if (_currentWaitingRecipeList.Count < _maxRecipeWaitingCount)
        {
            RecipeSO newRecipe = _availableRecipesList.AvailableRecipes[UnityEngine.Random.Range(0, _availableRecipesList.AvailableRecipes.Count)];
            _currentWaitingRecipeList.Add(newRecipe);
            OnAddRecipe?.Invoke();

            yield return new WaitForSeconds(UnityEngine.Random.Range(4f, 8f));
        }

        _isAddingNewRecipe = false;
    }

    public bool TryDeliverRecipePlate(Plate plate)
    {
        foreach (RecipeSO recipe in _currentWaitingRecipeList)
        {
            if (plate.Ingredients.Count != recipe.Ingredients.Count)
            {
                continue;
            }

            if (plate.Ingredients.OrderBy(ing => ing.ItemName).SequenceEqual(recipe.Ingredients.OrderBy(ing => ing.ItemName)))
            {
                _currentWaitingRecipeList.Remove(recipe);
                OnRemoveRecipe?.Invoke();
                OnDeliverySuccess?.Invoke();

                return true;
            }
        }

        OnDeliveryFailed?.Invoke();
        return false;
    }

    public List<RecipeSO> GetCurrentWaitingRecipeList() => _currentWaitingRecipeList;
}
