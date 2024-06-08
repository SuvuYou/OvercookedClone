public class CounterHolder : BaseCounter
{
    public override void Interact(KitchenItemParent player)
    {
        if (KitchenItemParent.TryAddIngredientToPlate(player, this))
        {
            DestroyCurrentItemHeld();
        }
        else
        {
            KitchenItemParent.SwapItemsOfTwoOwners(player, this);
        }
    }
}
