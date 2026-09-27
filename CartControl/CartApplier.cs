using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CartControl
{
    internal static class CartApplier
    {
        private static readonly FieldInfo ContainerWidth = AccessTools.Field(typeof(Container), "m_width");
        private static readonly FieldInfo ContainerHeight = AccessTools.Field(typeof(Container), "m_height");
        private static readonly FieldInfo InventoryWidth = AccessTools.Field(typeof(Inventory), "m_width");
        private static readonly FieldInfo InventoryHeight = AccessTools.Field(typeof(Inventory), "m_height");
        private static readonly MethodInfo UpdateMass = AccessTools.Method(typeof(Vagon), "UpdateMass");

        public static void Apply(Container container)
        {
            if (container == null)
            {
                return;
            }

            Vagon cart = container.GetComponentInParent<Vagon>();
            if (cart == null)
            {
                return;
            }

            CartCatalog.CartConfig config = CartCatalog.BindFromVagon(cart);
            if (config == null)
            {
                return;
            }

            int width = CartRules.ResolveWidth(config.Width.Value, config.VanillaWidth);
            int height = CartRules.ResolveHeight(config.Height.Value, config.VanillaHeight);
            if (ContainerWidth != null)
            {
                ContainerWidth.SetValue(container, width);
            }

            if (ContainerHeight != null)
            {
                ContainerHeight.SetValue(container, height);
            }

            Inventory inventory = container.GetInventory();
            if (inventory != null)
            {
                if (InventoryWidth != null)
                {
                    InventoryWidth.SetValue(inventory, width);
                }

                inventory.SetHeight(height);
                if (InventoryHeight != null)
                {
                    InventoryHeight.SetValue(inventory, height);
                }

                CartCatalog.RegisterInventory(inventory, CartCatalog.PrefabName(cart.gameObject));
            }

            ApplyPull(cart);
            object netView = AccessTools.Field(typeof(Vagon), "m_nview")?.GetValue(cart);
            if (netView == null || UpdateMass == null)
            {
                return;
            }

            try
            {
                UpdateMass.Invoke(cart, null);
            }
            catch (Exception ex)
            {
                Exception root = ex is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : ex;
                Plugin.LogError("Cart pull weight was not applied: " + root.Message);
            }
        }

        public static void ApplyPull(Vagon cart)
        {
            if (cart == null)
            {
                return;
            }

            CartCatalog.CartConfig config = CartCatalog.BindFromVagon(cart);
            if (config == null)
            {
                return;
            }

            float cargo = 0f;
            Container chest = cart.m_container;
            if (chest != null && chest.GetInventory() != null)
            {
                cargo = chest.GetInventory().GetTotalWeight();
            }

            float empty = CartRules.ResolveCartWeight(config.CartWeight.Value, config.VanillaCartWeight);
            float pull = CartRules.ResolveCargoPull(config.CargoPull.Value, config.VanillaCargoPull);
            cart.m_baseMass = empty;
            cart.m_itemWeightMassFactor = pull;
            Plugin.LogPull(cart, empty, pull, CartRules.ResolvePullMass(config.CartWeight.Value, config.VanillaCartWeight, config.CargoPull.Value, config.VanillaCargoPull, cargo));
        }

        public static void ApplyAllLoaded()
        {
            Container[] chests = UnityEngine.Object.FindObjectsByType<Container>(FindObjectsSortMode.None);
            if (chests == null)
            {
                return;
            }

            foreach (Container chest in chests)
            {
                Apply(chest);
            }
        }
    }
}
