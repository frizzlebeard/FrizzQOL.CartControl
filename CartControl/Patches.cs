using System;
using HarmonyLib;
using UnityEngine;

namespace CartControl
{
    [HarmonyPatch(typeof(ZNetScene), "Awake")]
    internal static class ZNetSceneAwakePatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            CartCatalog.DiscoverFromScene(__instance);
            CartDeconstruct.ApplyPrefabs(__instance, Plugin.FeatureOn());
        }
    }

    [HarmonyPatch(typeof(Vagon), "UpdateMass")]
    internal static class VagonUpdateMassPatch
    {
        private static void Prefix(Vagon __instance)
        {
            try
            {
                CartApplier.ApplyPull(__instance);
            }
            catch (Exception ex)
            {
                Plugin.LogError("Cart pull weight was not applied: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(WearNTear), "ApplyDamage")]
    internal static class CartBreakPatch
    {
        private static bool Prefix(WearNTear __instance, ref bool __result)
        {
            if (Plugin.Invincible == null || !CartRules.BlocksBreak(Plugin.Invincible.Value))
            {
                return true;
            }

            if (!IsCart(__instance))
            {
                return true;
            }

            __result = false;
            return false;
        }

        private static bool IsCart(WearNTear wear)
        {
            if (wear == null)
            {
                return false;
            }

            return wear.GetComponent<Vagon>() != null || wear.GetComponentInParent<Vagon>() != null;
        }
    }

    [HarmonyPatch(typeof(Vagon), "Awake")]
    internal static class VagonAwakePatch
    {
        private static void Postfix(Vagon __instance)
        {
            if (__instance != null)
            {
                CartDeconstruct.ApplyPiece(__instance.gameObject, Plugin.FeatureOn());
            }
        }
    }

    [HarmonyPatch(typeof(WearNTear), "UpdateWear")]
    internal static class WearNTearUpdateWearPatch
    {
        [ThreadStatic]
        private static int s_depth;

        internal static bool IsActive => s_depth > 0;

        private static void Prefix()
        {
            s_depth++;
        }

        private static Exception Finalizer(Exception __exception)
        {
            if (s_depth > 0)
            {
                s_depth--;
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(WearNTear), "RPC_Remove")]
    internal static class WearNTearRpcRemovePatch
    {
        [ThreadStatic]
        private static int s_depth;

        internal static bool IsActive => s_depth > 0;

        private static void Prefix()
        {
            s_depth++;
        }

        private static Exception Finalizer(Exception __exception)
        {
            if (s_depth > 0)
            {
                s_depth--;
            }

            return __exception;
        }
    }

    [HarmonyPatch(typeof(Piece), "CanBeRemoved")]
    internal static class PieceCanBeRemovedPatch
    {
        private static void Postfix(Piece __instance, ref bool __result)
        {
            try
            {
                if (WearNTearUpdateWearPatch.IsActive || __instance == null)
                {
                    return;
                }

                Vagon cart = CartDeconstruct.FindCart(__instance);
                bool? decision = CartRules.RemovalOverride(Plugin.FeatureOn(), cart != null, inUse: false, duringWear: false);
                if (decision.HasValue)
                {
                    __result = decision.Value;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogError("Cart remove check failed: " + ex.Message);
            }
        }
    }

    [HarmonyPatch(typeof(WearNTear), "Destroy")]
    internal static class WearNTearDestroyPatch
    {
        private static bool Prefix(WearNTear __instance, bool blockDrop)
        {
            try
            {
                return CartDeconstruct.AllowDestroy(
                    __instance,
                    Plugin.FeatureOn(),
                    WearNTearRpcRemovePatch.IsActive,
                    blockDrop);
            }
            catch (Exception ex)
            {
                Plugin.LogError("Cart remove failed: " + ex.Message);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Piece), "Awake")]
    internal static class PieceAwakePatch
    {
        private static void Postfix(Piece __instance)
        {
            if (__instance == null || !Plugin.FeatureOn())
            {
                return;
            }

            if (CartDeconstruct.FindCart(__instance) == null)
            {
                return;
            }

            __instance.m_canBeRemoved = true;
        }
    }

    [HarmonyPatch(typeof(Container), "Awake")]
    internal static class ContainerAwakePatch
    {
        private static void Postfix(Container __instance)
        {
            CartApplier.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(Inventory), "CanAddItem", typeof(ItemDrop.ItemData), typeof(int))]
    internal static class CanAddItemPatch
    {
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData __0, int __1, ref bool __result)
        {
            if (CargoGate.Allows(__instance, CargoGate.ItemWeight(__0, __1)))
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddItem", typeof(ItemDrop.ItemData))]
    internal static class AddItemPatch
    {
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData __0)
        {
            return CargoGate.Allows(__instance, CargoGate.ItemWeight(__0, __0 != null ? __0.m_stack : 0));
        }
    }

    internal static class CargoGate
    {
        public static bool Allows(Inventory inventory, float itemWeight)
        {
            CartCatalog.CartConfig config = CartCatalog.GetForInventory(inventory);
            if (config == null)
            {
                return true;
            }

            return CartRules.CanAddWeight(inventory.GetTotalWeight(), itemWeight, config.CargoWeight.Value);
        }

        public static float ItemWeight(ItemDrop.ItemData item, int stack)
        {
            if (item == null || item.m_shared == null)
            {
                return 0f;
            }

            int count = stack > 0 ? stack : 1;
            return item.m_shared.m_weight * count;
        }
    }
}
