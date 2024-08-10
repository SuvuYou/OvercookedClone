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
    private NetworkList<int> _currentWaitingRecipeIndicesList;

    private const int MAX_RECIPE_WAITING_COUNT = 4; 
    private const float MIN_TIMEOUT_BETWEEN_RECIPE_ADDED = 4f; 
    private const float MAX_TIMEOUT_BETWEEN_RECIPE_ADDED = 8f; 
    private TimingTimer _addingRecipeTimer = new(minDefaultTimerValue: MIN_TIMEOUT_BETWEEN_RECIPE_ADDED, maxDefaultTimerValue: MAX_TIMEOUT_BETWEEN_RECIPE_ADDED);

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            _currentWaitingRecipeIndicesList = new NetworkList<int> ();
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
            if (_currentWaitingRecipeIndicesList.Count != _currentWaitingRecipeList.Count)
            {
                _currentWaitingRecipeList.Clear();

                foreach (int index in _currentWaitingRecipeIndicesList)
                {
                    _currentWaitingRecipeList.Add(_availableRecipesList.AvailableRecipes[index]);
                }

                OnAddRecipe?.Invoke();
            }
        }

        if (IsServer)
        {
            _addingRecipeTimer.SubtractTime(Time.deltaTime);

            if (_addingRecipeTimer.IsTimerUp() && _currentWaitingRecipeList.Count < MAX_RECIPE_WAITING_COUNT)
            {
                _addingRecipeTimer.ResetTimer();

                int randRecipeIndex = UnityEngine.Random.Range(0, _availableRecipesList.AvailableRecipes.Count);
                _currentWaitingRecipeIndicesList.Add(randRecipeIndex);
            }
        }
    }

    private void _deliverSuccessfulRecipe()
    {
        OnRemoveRecipe?.Invoke();
        OnDeliverySuccess?.Invoke();
    }

    private void _deliverFailedRecipe()
    {
        OnDeliveryFailed?.Invoke(); 
    }

    [ClientRpc]
    private void _deliverSuccessfulRecipeClientRpc(ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return; 

        _deliverSuccessfulRecipe();
    }

    [ClientRpc]
    private void _deliverFailedRecipeClientRpc(ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return; 

        _deliverFailedRecipe();
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
                _deliverSuccessfulRecipe();
                _deliverSuccessfulRecipeServerRpc(_currentWaitingRecipeList.IndexOf(recipe));

                return true;
            }
        }

        _deliverFailedRecipe();
        _deliverFailedRecipeServerRpc();

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void _deliverSuccessfulRecipeServerRpc(int recipeIndexAtNetworkList, ServerRpcParams rpcParams = default)
    {
        _currentWaitingRecipeIndicesList.RemoveAt(recipeIndexAtNetworkList);
        _deliverSuccessfulRecipeClientRpc(senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void _deliverFailedRecipeServerRpc(ServerRpcParams rpcParams = default)
    {
        _deliverFailedRecipeClientRpc(senderClientId: rpcParams.Receive.SenderClientId);
    }

    public List<RecipeSO> GetCurrentWaitingRecipeList() => _currentWaitingRecipeList;
}
