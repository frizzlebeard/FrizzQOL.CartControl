#nullable disable
public static class CartRules
{
    public const int MaxWidth = 8;

    public static int ResolveWidth(int config, int vanilla)
    {
        if (config <= 0)
        {
            return vanilla;
        }

        if (config > MaxWidth)
        {
            return MaxWidth;
        }

        return config;
    }

    public static int ResolveHeight(int config, int vanilla)
    {
        return config <= 0 ? vanilla : config;
    }

    public static float ResolveCartWeight(float config, float vanilla)
    {
        return config < 0f ? vanilla : config;
    }

    public static float ResolveCargoPull(float config, float vanilla)
    {
        return config < 0f ? vanilla : config;
    }

    public static float ResolvePullMass(float cartWeight, float vanillaCart, float cargoPull, float vanillaPull, float cargoWeight)
    {
        float empty = ResolveCartWeight(cartWeight, vanillaCart);
        float pull = ResolveCargoPull(cargoPull, vanillaPull);
        float cargo = cargoWeight > 0f ? cargoWeight : 0f;
        return empty + (cargo * pull);
    }

    public static bool BlocksBreak(bool invincible)
    {
        return invincible;
    }

    public static bool? RemovalOverride(bool featureOn, bool isCart, bool inUse, bool duringWear)
    {
        if (duringWear || !featureOn || !isCart || inUse)
        {
            return null;
        }

        return true;
    }

    public static bool ShouldSpillCargo(bool featureOn, bool isCart, bool hammerRemove, bool blockDrop)
    {
        return featureOn && isCart && hammerRemove && !blockDrop;
    }

    public static bool CanAddWeight(float currentWeight, float itemWeight, float maxWeight)
    {
        if (maxWeight <= 0f)
        {
            return true;
        }

        return currentWeight + itemWeight <= maxWeight;
    }
}
