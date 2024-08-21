using System;
using System.Collections.Generic;
using Unity.Netcode;

public class Plate : KitchenItem, IPlate
{
    public event Action<List<KitchenItemSO>> OnIngredientsChange;
    public event Action OnDeliverPlate;
    static private List<KitchenItemSO> AllowedIngredients;
    public List<KitchenItemSO> Ingredients {get; private set;}
    
    private NetworkList<int> _ingredientsIndices;

    private void Awake()
    {
        Ingredients = new();
        _ingredientsIndices = new NetworkList<int> ();
    }

    public static void InitAllowedIngridients(List<KitchenItemSO> allowedIngredients)
    {
        AllowedIngredients = allowedIngredients;
    }

    public void DeliverPlate()
    {
        _triggerOnDeliverPlateServerRpc();
        OnDeliverPlate?.Invoke();
    }

    private void Update()
    {
        if (IsClient)
        {
            if (_ingredientsIndices.Count != Ingredients.Count)
            {
                Ingredients.Clear();

                foreach (int index in _ingredientsIndices)
                {
                    Ingredients.Add(AllowedIngredients[index]);  
                }

                OnIngredientsChange?.Invoke(Ingredients);
            }
        }
    }

    public static bool IsIngridientAllowedOnPlate(KitchenItemSO ingredient)
    {
        return AllowedIngredients.Contains(ingredient);
    }

    public void ClearAllIngredientsOnNetwork()
    {
        _clearAllIngredientsServerRpc();
    }

    public bool CanAddIngredient(KitchenItemSO ingredient)
    {
        return AllowedIngredients.Contains(ingredient) && !Ingredients.Contains(ingredient);
    }
    
    public void AddIngredientOnNetwork(KitchenItemSO ingredient)
    {
        _addIngredientServerRpc(KitchenItemsList.Instance.GetIndexOfItem(ingredient));
    }

    [ServerRpc(RequireOwnership = false)]
    private void _clearAllIngredientsServerRpc()
    {
        _ingredientsIndices.Clear();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _triggerOnDeliverPlateServerRpc(ServerRpcParams rpcParams = default)
    {
        _triggerOnDeliverPlateClientRpc(senderClientId: rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void _triggerOnDeliverPlateClientRpc(ulong senderClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == senderClientId) return;
        OnDeliverPlate?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    private void _addIngredientServerRpc(int kitchenItemIndex)
    {
        KitchenItemSO ingredient = KitchenItemsList.Instance.Items[kitchenItemIndex];
        _ingredientsIndices.Add(AllowedIngredients.IndexOf(ingredient));
    }
}
