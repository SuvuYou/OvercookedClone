using System.Collections.Generic;
using UnityEngine;

public class KitchenItemsList : MonoBehaviour
{
    public static KitchenItemsList Instance;

    [SerializeField] private List<KitchenItemSO> _kitchenItemsList;

    public List<KitchenItemSO> Items {get {return _kitchenItemsList; } }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);

            return;
        }

        Instance = this;
    }

    public int GetIndexOfItem(KitchenItemSO item)
    {
        return Items.IndexOf(item);
    }
}
