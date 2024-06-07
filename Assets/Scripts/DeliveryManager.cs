using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance;

    public event Action OnAddRecipe;
    public event Action OnRemoveRecipe;
    public event Action OnDeliverySuccess;
    public event Action OnDeliveryFailed;

    [SerializeField] private AvailableRecipesListSO _availableRecipesList;
    private List<RecipeSO> _currentWaitingRecipeList = new();
    private NetworkList<int> _currentWaitingRecipeIndeciesList;

    private const int MAX_RECIPE_WAITING_COUNT = 4; 
    // TODO: possible make random
    private const float TIMEOUT_BETWEEN_RECIPE_ADDED = 4f; 
    private TimingTimer _addingRecipeTimer = new(defaultTimerValue: TIMEOUT_BETWEEN_RECIPE_ADDED);

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            _currentWaitingRecipeIndeciesList = new NetworkList<int>();
            Instance = this;
        }
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Active)
        {
            return;
        }

        if (IsClient)
        {
            if (_currentWaitingRecipeList.Count != _currentWaitingRecipeIndeciesList.Count)
            {
                _syncRecipeListWithNetworkList();
            }
        }

        if (IsServer)
        {
            _addingRecipeTimer.SubtractTime(Time.deltaTime);

            if (_addingRecipeTimer.Timer <= 0 && _currentWaitingRecipeList.Count < MAX_RECIPE_WAITING_COUNT)
            {
                _addingRecipeTimer.ResetTimer();

                int randRecipeIndex = UnityEngine.Random.Range(0, _availableRecipesList.AvailableRecipes.Count);
                _currentWaitingRecipeIndeciesList.Add(randRecipeIndex);
            }
        }
    }

    private void _syncRecipeListWithNetworkList()
    {
        _currentWaitingRecipeList.Clear();

        foreach (int recipeIndex in _currentWaitingRecipeIndeciesList)
        {
            _addRecipeToWaitingListByIndex(recipeIndex);   
        }

        OnAddRecipe?.Invoke();
    }

    private void _addRecipeToWaitingListByIndex(int recipeIndex)
    {
        RecipeSO newRecipe = _availableRecipesList.AvailableRecipes[recipeIndex];
        _currentWaitingRecipeList.Add(newRecipe);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _deliverSuccessfulRecipeServerRpc(int recipeIndexAtNetworkList)
    {
        _currentWaitingRecipeIndeciesList.RemoveAt(recipeIndexAtNetworkList);
        _deliverSuccessfulRecipeClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _deliverFailedRecipeServerRpc()
    {
        _deliverFailedRecipeClientRpc();
    }

    [ClientRpc]
    private void _deliverSuccessfulRecipeClientRpc()
    {
        OnRemoveRecipe?.Invoke();
        OnDeliverySuccess?.Invoke();
    }

    [ClientRpc]
    private void _deliverFailedRecipeClientRpc()
    {
        OnDeliveryFailed?.Invoke(); 
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
                _deliverSuccessfulRecipeServerRpc(_currentWaitingRecipeList.FindIndex((RecipeSO match) => match == recipe));
                return true;
            }
        }

        _deliverFailedRecipeServerRpc();
        return false;
    }

    public List<RecipeSO> GetCurrentWaitingRecipeList() => _currentWaitingRecipeList;
}
