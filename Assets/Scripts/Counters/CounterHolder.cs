public class CounterHolder : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (!KitchenItemParent.TryAddIngredientToPlateOwner(player, this))
        {
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }
}
