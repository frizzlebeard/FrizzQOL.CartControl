using Xunit;

public class CartRulesTests
{
    [Fact]
    public void Width_uses_config_and_caps_at_eight()
    {
        Assert.Equal(6, CartRules.ResolveWidth(6, 5));
        Assert.Equal(8, CartRules.ResolveWidth(20, 5));
        Assert.Equal(5, CartRules.ResolveWidth(0, 5));
    }

    [Fact]
    public void Height_zero_keeps_vanilla()
    {
        Assert.Equal(4, CartRules.ResolveHeight(4, 3));
        Assert.Equal(3, CartRules.ResolveHeight(0, 3));
    }

    [Fact]
    public void Cart_weight_allows_zero_and_negative_keeps_vanilla()
    {
        Assert.Equal(0f, CartRules.ResolveCartWeight(0f, 20f));
        Assert.Equal(8f, CartRules.ResolveCartWeight(8f, 20f));
        Assert.Equal(20f, CartRules.ResolveCartWeight(-1f, 20f));
    }

    [Fact]
    public void Cargo_pull_scales_loaded_weight_and_negative_keeps_vanilla()
    {
        Assert.Equal(0.25f, CartRules.ResolveCargoPull(0.25f, 1f));
        Assert.Equal(0f, CartRules.ResolveCargoPull(0f, 1f));
        Assert.Equal(1f, CartRules.ResolveCargoPull(-1f, 1f));
        Assert.Equal(75f, CartRules.ResolvePullMass(50f, 50f, 0.25f, 1f, 100f));
        Assert.Equal(50f, CartRules.ResolvePullMass(50f, 20f, 0f, 1f, 400f));
    }

    [Fact]
    public void Hammer_remove_allows_a_loaded_cart()
    {
        Assert.True(CartRules.RemovalOverride(true, true, false, false));
        Assert.Null(CartRules.RemovalOverride(true, true, true, false));
        Assert.Null(CartRules.RemovalOverride(false, true, false, false));
        Assert.Null(CartRules.RemovalOverride(true, false, false, false));
        Assert.Null(CartRules.RemovalOverride(true, true, false, true));
    }

    [Fact]
    public void Hammer_remove_spills_cart_cargo()
    {
        Assert.True(CartRules.ShouldSpillCargo(true, true, true, false));
        Assert.False(CartRules.ShouldSpillCargo(true, true, false, false));
        Assert.False(CartRules.ShouldSpillCargo(true, false, true, false));
        Assert.False(CartRules.ShouldSpillCargo(false, true, true, false));
    }

    [Fact]
    public void Invincible_on_blocks_break_and_off_allows_it()
    {
        Assert.True(CartRules.BlocksBreak(true));
        Assert.False(CartRules.BlocksBreak(false));
    }

    [Fact]
    public void Cargo_cap_blocks_overweight_and_zero_means_no_cap()
    {
        Assert.False(CartRules.CanAddWeight(90f, 20f, 100f));
        Assert.True(CartRules.CanAddWeight(90f, 10f, 100f));
        Assert.True(CartRules.CanAddWeight(500f, 50f, 0f));
    }
}
