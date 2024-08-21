using System;
using System.Collections.Generic;

public interface IPlate
{
    public event Action<List<KitchenItemSO>> OnIngredientsChange;
    public event Action OnDeliverPlate;
}
