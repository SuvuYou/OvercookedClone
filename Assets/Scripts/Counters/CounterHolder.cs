public class CounterHolder : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (!KitchenItemParent.TryAddIngredientToPlate(player, this))
        {
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }
}
