using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CartControl
{
    internal static class CartDeconstruct
    {
        private static readonly Dictionary<string, bool> Originals = new Dictionary<string, bool>();
        private static readonly FieldInfo ContainerNview = AccessTools.Field(typeof(Container), "m_nview");
        private static readonly MethodInfo DropAllItems = AccessTools.Method(typeof(Container), "DropAllItems", Type.EmptyTypes);

        internal static Vagon FindCart(Component component)
        {
            if (component == null)
            {
                return null;
            }

            Vagon cart = component.GetComponent<Vagon>();
            if (cart != null)
            {
                return cart;
            }

            return component.GetComponentInParent<Vagon>();
        }

        internal static void ApplyAllLoaded(bool featureOn)
        {
            if (ZNetScene.instance != null)
            {
                ApplyPrefabs(ZNetScene.instance, featureOn);
            }

            Vagon[] carts = UnityEngine.Object.FindObjectsByType<Vagon>(FindObjectsSortMode.None);
            if (carts == null)
            {
                return;
            }

            for (int i = 0; i < carts.Length; i++)
            {
                if (carts[i] != null)
                {
                    ApplyPiece(carts[i].gameObject, featureOn);
                }
            }
        }

        internal static void ApplyPrefabs(ZNetScene scene, bool featureOn)
        {
            if (scene == null)
            {
                return;
            }

            List<GameObject> prefabs = AccessTools.Field(typeof(ZNetScene), "m_prefabs")?.GetValue(scene) as List<GameObject>;
            if (prefabs == null)
            {
                return;
            }

            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab != null && prefab.GetComponent<Vagon>() != null)
                {
                    ApplyPiece(prefab, featureOn);
                }
            }
        }

        internal static void ApplyPiece(GameObject cartObject, bool featureOn)
        {
            if (cartObject == null)
            {
                return;
            }

            Piece[] pieces = cartObject.GetComponentsInChildren<Piece>(true);
            if (pieces == null || pieces.Length == 0)
            {
                Piece parent = cartObject.GetComponentInParent<Piece>();
                if (parent == null)
                {
                    return;
                }

                pieces = new Piece[] { parent };
            }

            string name = CartCatalog.PrefabName(cartObject);
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            for (int i = 0; i < pieces.Length; i++)
            {
                Piece piece = pieces[i];
                if (piece == null)
                {
                    continue;
                }

                if (!Originals.ContainsKey(name))
                {
                    Originals[name] = piece.m_canBeRemoved;
                }

                piece.m_canBeRemoved = featureOn || Originals[name];
            }
        }

        internal static bool AllowDestroy(WearNTear wear, bool featureOn, bool hammerRemove, bool blockDrop)
        {
            if (!featureOn || !hammerRemove || blockDrop)
            {
                return true;
            }

            Vagon cart = FindCart(wear);
            if (!CartRules.ShouldSpillCargo(featureOn, cart != null, hammerRemove, blockDrop))
            {
                return true;
            }

            if (DropAllItems == null)
            {
                Plugin.LogError("Container.DropAllItems() was not found. Cart was not removed.");
                return false;
            }

            Container[] chests = cart.GetComponentsInChildren<Container>(true);
            for (int i = 0; i < chests.Length; i++)
            {
                if (!TryDrop(chests[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryDrop(Container chest)
        {
            if (chest == null || chest.GetInventory() == null)
            {
                return true;
            }

            ZNetView view = ContainerNview?.GetValue(chest) as ZNetView;
            if (view == null || !view.IsValid() || !view.IsOwner())
            {
                return true;
            }

            try
            {
                DropAllItems.Invoke(chest, null);
                return true;
            }
            catch (Exception ex)
            {
                Exception root = ex is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : ex;
                Plugin.LogError("Cargo drop failed. Cart was not removed: " + root.Message);
                return false;
            }
        }
    }
}
